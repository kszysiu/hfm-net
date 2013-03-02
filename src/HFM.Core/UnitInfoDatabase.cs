﻿/*
 * HFM.NET - Work Unit History Database
 * Copyright (C) 2009-2013 Ryan Harlamert (harlam357)
 *
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation; version 2
 * of the License. See the included file GPLv2.TXT.
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with this program; if not, write to the Free Software
 * Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.
 */
   
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic;
using System.Data;
using System.Data.Common;
using System.Data.SQLite;
using System.Diagnostics;
using System.Globalization;
using System.Text.RegularExpressions;

using Castle.Core.Logging;

using HFM.Core.DataTypes;

namespace HFM.Core
{
   public enum SqlTable
   {
      WuHistory,
      Version
   }

   public interface IUnitInfoDatabase : IDisposable
   {
      /// <summary>
      /// Flag that notes if the Database is safe to call.
      /// </summary>
      bool Connected { get; }

      SQLiteConnection Connection { get; }

      void Insert(UnitInfoLogic unitInfoLogic);

      int Delete(HistoryEntry entry);

      IList<HistoryEntry> Fetch(QueryParameters parameters);

      IList<HistoryEntry> Fetch(QueryParameters parameters, HistoryProductionView productionView);

      bool TableExists(SqlTable sqlTable);

      DataTable Select(string sql);

      int Execute(string sql);
   }

   public sealed class UnitInfoDatabase : IUnitInfoDatabase
   {
      #region Fields

      public bool ForceDateTimesToUtc { get; set; }

      private string ConnectionString
      {
         get { return @"Data Source=" + DatabaseFilePath + (ForceDateTimesToUtc ? ";DateTimeKind=Utc" : String.Empty); }
      }

      //private const string DbProvider = "System.Data.SQLite";

      private string _databaseFilePath;
      /// <summary>
      /// Get or Set the Database File Path
      /// </summary>
      public string DatabaseFilePath
      {
         get { return _databaseFilePath; }
         set
         {
            _databaseFilePath = value;
            CheckConnection();
         }
      }

      /// <summary>
      /// Flag that notes if the Database is safe to call
      /// </summary>
      public bool Connected { get; private set; }

      private readonly ILogger _logger = NullLogger.Instance;

      private readonly IProteinDictionary _proteinDictionary;
      private static readonly Dictionary<SqlTable, SqlTableCommands> SqlTableCommandDictionary = new Dictionary<SqlTable, SqlTableCommands>
                                                                                                 {
                                                                                                    { SqlTable.WuHistory, new WuHistorySqlTableCommands() },
                                                                                                    { SqlTable.Version, new VersionSqlTableCommands() }
                                                                                                 };  

      private SQLiteConnection _connection;

      public SQLiteConnection Connection
      {
         get { return _connection; }
      }

      private PetaPoco.Database _database;

      #endregion

      #region Constructor

      public UnitInfoDatabase(IPreferenceSet prefs, IProteinDictionary proteinDictionary)
         : this(prefs, proteinDictionary, null)
      {

      }

      public UnitInfoDatabase(IPreferenceSet prefs, IProteinDictionary proteinDictionary, ILogger logger)
      {
         if (proteinDictionary == null) throw new ArgumentNullException("proteinDictionary");
         _proteinDictionary = proteinDictionary;

         if (logger != null)
         {
            _logger = logger;
         }

         ForceDateTimesToUtc = true;
         if (prefs != null && !String.IsNullOrEmpty(prefs.ApplicationDataFolderPath))
         {
            DatabaseFilePath = System.IO.Path.Combine(prefs.ApplicationDataFolderPath, Constants.SqLiteFilename);
         }
      }

      #endregion

      #region Methods

      /// <summary>
      /// Check the Database Connection
      /// </summary>
      private void CheckConnection()
      {
         try
         {
            if (_connection != null)
            {
               _connection.Dispose();
            }
            _connection = new SQLiteConnection(ConnectionString);
            _connection.Open();
            if (_database != null)
            {
               _database.Dispose();
            }
            _database = new PetaPoco.Database(_connection);

            bool exists = TableExists(SqlTable.WuHistory);
            if (!exists)
            {
               CreateTable(SqlTable.WuHistory);
               CreateTable(SqlTable.Version);
               SetDatabaseVersion(Application.VersionWithRevision);
            }
            var parameters = new QueryParameters();
            parameters.Fields.Add(new QueryField
                                    {
                                       Name = QueryFieldName.ID,
                                       Type = QueryFieldType.Equal,
                                       Value = 0
                                    });
            if (exists)
            {
               PerformUpgrade();
            }
            Fetch(parameters);
            Connected = true;
         }
         catch (Exception ex)
         {
            _logger.ErrorFormat(ex, "{0}", ex.Message);

            if (_connection != null)
            {
               _connection.Dispose();
            }
            if (_database != null)
            {
               _database.Dispose();
            }
            Connected = false;
         }
      }

      #region Database Upgrades

      private void PerformUpgrade()
      {
         string dbVersionString = "0.0.0.0";
         if (TableExists(SqlTable.Version))
         {
            dbVersionString = GetDatabaseVersion();
         }
         else
         {
            CreateTable(SqlTable.Version);
         }
         int dbVersion = Application.ParseVersion(dbVersionString);
         _logger.Info("WU History database v{0}", dbVersionString);

         bool upgraded = false;
         const string upgradeVersion1String = "0.9.2.0";
         if (dbVersion < Application.ParseVersion(upgradeVersion1String))
         {
            _logger.Info("Performing WU History database upgrade to v{0}...", upgradeVersion1String);
            DeleteDuplicates(_connection);
            UpgradeWuHistory1();
            upgraded = true;
         }

         if (upgraded)
         {
            SetDatabaseVersion(Application.VersionWithRevision);
         }
      }

      private void DeleteDuplicates(SQLiteConnection con)
      {
         Debug.Assert(TableExists(SqlTable.WuHistory));
         Debug.Assert(con.State.Equals(ConnectionState.Open));

         const string select =
            "SELECT ID, ProjectID, ProjectRun, ProjectClone, ProjectGen, DownloadDateTime, COUNT(*) " +
            "FROM WuHistory " +
            "GROUP BY ProjectID, ProjectRun, ProjectClone, ProjectGen, DownloadDateTime " +
            "HAVING COUNT(*) > 1 ";
            //"ORDER BY ID ASC";

         const string delete =
            "DELETE FROM WuHistory " +
            "WHERE ID < @0 AND ProjectID = @1 AND ProjectRun = @2 AND ProjectClone = @3 AND ProjectGen = @4 AND datetime(DownloadDateTime) = datetime(@5)";

         int count = 0;
         _logger.Info("Checking for duplicate WU History entries...");

         using (var adapter = new SQLiteDataAdapter(select, con))
         using (var table = new DataTable())
         {
            adapter.Fill(table);
            foreach (DataRow row in table.Rows)
            {
               using (var cmd = con.CreateCommand())
               {
                  cmd.CommandText = delete;
                  for (int i = 0; i < row.ItemArray.Length; i++)
                  {
                     var param = cmd.CreateParameter();
                     param.ParameterName = "@" + i;
                     param.Value = row.ItemArray[i];
                     cmd.Parameters.Add(param);
                  }
                  int result = cmd.ExecuteNonQuery();
                  if (result != 0)
                  {
                     _logger.Debug("Deleted rows: {0}", result);
                     count += result;
                  }
               }
            }
         }

         if (count != 0)
         {
            _logger.Info("Total number of duplicate WU History entries deleted: {0}", count);
         }
      }

      private void UpgradeWuHistory1()
      {
         var adder = new SQLiteColumnAdder
         {
            TableName = SqlTableCommandDictionary[SqlTable.WuHistory].TableName,
            Connection = _connection
         };

         adder.AddColumn("WorkUnitName", "VARCHAR(30)");
         adder.AddColumn("KFactor", "FLOAT");
         adder.AddColumn("Core", "VARCHAR(20)");
         adder.AddColumn("Frames", "INT");
         adder.AddColumn("Atoms", "INT");
         adder.AddColumn("SlotType", "VARCHAR(20)");
         adder.AddColumn("Credit", "FLOAT");
         adder.AddColumn("PreferredDays", "FLOAT");
         adder.AddColumn("MaximumDays", "FLOAT");
         adder.Execute();
      }

      private void SetDatabaseVersion(string version)
      {
         Debug.Assert(!String.IsNullOrWhiteSpace(version));

         _logger.Info("Setting database version to: v{0}", version);
         using (var cmd = new SQLiteCommand("INSERT INTO [DbVersion] (Version) VALUES (?);", _connection))
         {
            var param = new SQLiteParameter("Version", DbType.String) { Value = version };
            cmd.Parameters.Add(param);
            cmd.ExecuteNonQuery();
         }
      }

      #endregion

      #region Insert
   
      public void Insert(UnitInfoLogic unitInfoLogic)
      {
         // validate the UnitInfoLogic before opening the connection
         if (!ValidateUnitInfo(unitInfoLogic.UnitInfoData)) return;

         Debug.Assert(TableExists(SqlTable.WuHistory));

         // ensure this unit is not written twice
         if (!UnitInfoExists(unitInfoLogic))
         {
            var entry = AutoMapper.Mapper.Map<HistoryEntry>(unitInfoLogic.UnitInfoData);
            entry.FramesCompleted = unitInfoLogic.FramesComplete;
            entry.FrameTimeValue = unitInfoLogic.GetRawTime(PpdCalculationType.AllFrames);
            entry.SetProtein(unitInfoLogic.CurrentProtein);
            _database.Insert(entry);
         }
      }

      private static bool ValidateUnitInfo(UnitInfo unitInfo)
      {
         bool result = ValidateFinishedUnitInfo(unitInfo);
         if (!result)
         {
            // Issue 233
            result = ValidateIncompleteUnitInfo(unitInfo);
         }
         return result;
      }

      private static bool ValidateFinishedUnitInfo(UnitInfo unitInfo)
      {
         return unitInfo.ProjectIsUnknown() == false &&
                unitInfo.UnitResult.Equals(WorkUnitResult.FinishedUnit) &&
                unitInfo.DownloadTime.Equals(DateTime.MinValue) == false &&
                unitInfo.FinishedTime.Equals(DateTime.MinValue) == false;
      }

      private static bool ValidateIncompleteUnitInfo(UnitInfo unitInfo)
      {
         // Finished Time will not be populated if any of these error
         // results are detected.  Only check for valid Project and 
         // download time - Issue 233

         return unitInfo.ProjectIsUnknown() == false &&
               (unitInfo.UnitResult.Equals(WorkUnitResult.BadWorkUnit) ||
                unitInfo.UnitResult.Equals(WorkUnitResult.CoreOutdated) ||
                unitInfo.UnitResult.Equals(WorkUnitResult.EarlyUnitEnd) ||
                unitInfo.UnitResult.Equals(WorkUnitResult.Interrupted) ||
                unitInfo.UnitResult.Equals(WorkUnitResult.UnstableMachine)) &&
                unitInfo.DownloadTime.Equals(DateTime.MinValue) == false;
      }

      private bool UnitInfoExists(UnitInfoLogic unitInfoLogic)
      {
         var rows = Fetch(BuildUnitKeyQueryParameters(unitInfoLogic));
         return rows.Count != 0;
      }

      private static QueryParameters BuildUnitKeyQueryParameters(UnitInfoLogic unitInfoLogic)
      {
         var parameters = new QueryParameters();
         parameters.Fields.Add(new QueryField { Name = QueryFieldName.ProjectID, Type = QueryFieldType.Equal, Value = unitInfoLogic.UnitInfoData.ProjectID });
         parameters.Fields.Add(new QueryField { Name = QueryFieldName.ProjectRun, Type = QueryFieldType.Equal, Value = unitInfoLogic.UnitInfoData.ProjectRun });
         parameters.Fields.Add(new QueryField { Name = QueryFieldName.ProjectClone, Type = QueryFieldType.Equal, Value = unitInfoLogic.UnitInfoData.ProjectClone });
         parameters.Fields.Add(new QueryField { Name = QueryFieldName.ProjectGen, Type = QueryFieldType.Equal, Value = unitInfoLogic.UnitInfoData.ProjectGen });
         parameters.Fields.Add(new QueryField { Name = QueryFieldName.DownloadDateTime, Type = QueryFieldType.Equal, Value = unitInfoLogic.UnitInfoData.DownloadTime });
         return parameters;
      }

      #endregion

      #region Delete
      
      public int Delete(HistoryEntry entry)
      {
         Debug.Assert(TableExists(SqlTable.WuHistory));

         return _database.Delete(entry);
      }

      #endregion

      #region Fetch
      
      public IList<HistoryEntry> Fetch(QueryParameters parameters)
      {
         return Fetch(parameters, HistoryProductionView.BonusDownloadTime);
      }
      
      public IList<HistoryEntry> Fetch(QueryParameters parameters, HistoryProductionView productionView)
      {
         DateTime start = Instrumentation.ExecStart;
         try
         {
            return FetchInternal(parameters, productionView);
         }
         finally
         {
            _logger.Debug("Database Fetch ({0}) completed in {1}", parameters, Instrumentation.GetExecTime(start));
         }
      }

      private IList<HistoryEntry> FetchInternal(QueryParameters parameters, HistoryProductionView productionView)
      {
         Debug.Assert(TableExists(SqlTable.WuHistory));

         PetaPoco.Sql where = WhereBuilder.Execute(parameters);
         List<HistoryEntry> query = where != null ? _database.Fetch<HistoryEntry>(where) : _database.Fetch<HistoryEntry>(String.Empty);
         Debug.Assert(query != null);
         query.ForEach(x => x.ProductionView = productionView);

         if (_proteinDictionary == null) return query;
            
         var joinQuery = from entry in query
                           join protein in _proteinDictionary.Values on entry.ProjectID equals protein.ProjectNumber into groupJoin
                           from entryProtein in groupJoin.DefaultIfEmpty()
                           select entry.SetProtein(entryProtein);

         return FilterProteinParameters(parameters, joinQuery);
      }

      private IList<HistoryEntry> FilterProteinParameters(QueryParameters parameters, IEnumerable<HistoryEntry> entries)
      {
         var query = entries.AsQueryable();

         foreach (var field in parameters.Fields)
         {
            if (field.Name.Equals(QueryFieldName.WorkUnitName) ||
                field.Name.Equals(QueryFieldName.KFactor) ||
                field.Name.Equals(QueryFieldName.Core) ||
                field.Name.Equals(QueryFieldName.Frames) ||
                field.Name.Equals(QueryFieldName.Atoms) ||
                field.Name.Equals(QueryFieldName.SlotType) ||
                field.Name.Equals(QueryFieldName.PPD) ||
                field.Name.Equals(QueryFieldName.Credit))
            {
               if (field.Type.Equals(QueryFieldType.Like))
               {
                  query = Like(query, field);
               }
               else if (field.Type.Equals(QueryFieldType.NotLike))
               {
                  query = NotLike(query, field);
               }
               else
               {
                  try
                  {
                     query = query.Where(BuildWhereCondition(field));
                  }
                  catch (ParseException ex)
                  {
                     _logger.WarnFormat(ex, "{0}", ex.Message);
                  }
               }
            }
         }

         return query.ToList();
      }

      private static IQueryable<HistoryEntry> Like(IQueryable<HistoryEntry> query, QueryField field)
      {
         Debug.Assert(field.Type.Equals(QueryFieldType.Like));
         return LikeMatch(query, field, IsSqlLikeMatch);
      }

      private static IQueryable<HistoryEntry> NotLike(IQueryable<HistoryEntry> query, QueryField field)
      {
         Debug.Assert(field.Type.Equals(QueryFieldType.NotLike));
         return LikeMatch(query, field, (input, pattern) => !IsSqlLikeMatch(input, pattern));
      }
      
      private static IQueryable<HistoryEntry> LikeMatch(IQueryable<HistoryEntry> query, QueryField field, Func<string, string, bool> func)
      {
         Debug.Assert(field.Type.Equals(QueryFieldType.Like) ||
                      field.Type.Equals(QueryFieldType.NotLike));

         if (field.Name.Equals(QueryFieldName.WorkUnitName))
         {
            return query.Where(x => func(x.WorkUnitName, field.Value.ToString()));
         }
         if (field.Name.Equals(QueryFieldName.KFactor))
         {
            return query.Where(x => func(x.KFactor.ToString(), field.Value.ToString()));
         }
         if (field.Name.Equals(QueryFieldName.Core))
         {
            return query.Where(x => func(x.Core, field.Value.ToString()));
         }
         if (field.Name.Equals(QueryFieldName.Frames))
         {
            return query.Where(x => func(x.Frames.ToString(), field.Value.ToString()));
         }
         if (field.Name.Equals(QueryFieldName.Atoms))
         {
            return query.Where(x => func(x.Atoms.ToString(), field.Value.ToString()));
         }
         if (field.Name.Equals(QueryFieldName.SlotType))
         {
            return query.Where(x => func(x.SlotType, field.Value.ToString()));
         }
         if (field.Name.Equals(QueryFieldName.PPD))
         {
            return query.Where(x => func(x.PPD.ToString(), field.Value.ToString()));
         }
         if (field.Name.Equals(QueryFieldName.Credit))
         {
            return query.Where(x => func(x.Credit.ToString(), field.Value.ToString()));
         }

         // ReSharper disable HeuristicUnreachableCode
         Debug.Assert(false);
         return query;
         // ReSharper restore HeuristicUnreachableCode
      }

      private static bool IsSqlLikeMatch(string input, string pattern)
      {
         // Method from here: http://bytes.com/topic/c-sharp/answers/253519-using-regex-create-sqls-like-like-function

         /* Turn "off" all regular expression related syntax in
         * the pattern string. */
         pattern = Regex.Escape(pattern);

         /* Replace the SQL LIKE wildcard metacharacters with the
         * equivalent regular expression metacharacters. */
         pattern = pattern.Replace("%", ".*?").Replace("_", ".");

         /* The previous call to Regex.Escape actually turned off
         * too many metacharacters, i.e. those which are recognized by
         * both the regular expression engine and the SQL LIKE
         * statement ([...] and [^...]). Those metacharacters have
         * to be manually unescaped here. */
         pattern = pattern.Replace(@"\[", "[").Replace(@"\]", "]").Replace(@"\^", "^");

         // anchor the pattern - rwh 12/1/12
         if (!pattern.StartsWith("^"))
         {
            pattern = "^" + pattern;
         }
         if (!pattern.EndsWith("$"))
         {
            pattern = pattern + "$";
         }
         
         return input != null && Regex.IsMatch(input, pattern, RegexOptions.IgnoreCase | RegexOptions.Singleline);
      }

      private static string BuildWhereCondition(QueryField queryField)
      {
         string valueFormat = "{2}";
         if (queryField.Name.Equals(QueryFieldName.WorkUnitName) ||
             queryField.Name.Equals(QueryFieldName.Core) ||
             queryField.Name.Equals(QueryFieldName.SlotType))
         {
            valueFormat = "\"{2}\"";
         }

         return String.Format(CultureInfo.InvariantCulture, "{0} {1} " + valueFormat, queryField.Name, queryField.Operator, queryField.Value);
      }

      #endregion

      #region Select

      public DataTable Select(string sql)
      {
         using (var adapter = new SQLiteDataAdapter(sql, _connection))
         {
            var table = new DataTable();
            adapter.Fill(table);
            return table;
         }
      }

      #endregion

      #region Execute

      public int Execute(string sql)
      {
         using (var command = _connection.CreateCommand())
         {
            command.CommandText = sql;
            return command.ExecuteNonQuery();
         }
      }

      #endregion

      #region IDisposable Members

      private bool _disposed;

      public void Dispose()
      {
         Dispose(true);
         //GC.SuppressFinalize(this);
      }

      private void Dispose(bool disposing)
      {
         if (!_disposed)
         {
            if (disposing)
            {
               if (_connection != null)
               {
                  _connection.Dispose();
               }
               if (_database != null)
               {
                  _database.Dispose();
               }
            }
         }

         _disposed = true;
      }

      #endregion

      #region Table Helpers

      public bool TableExists(SqlTable sqlTable)
      {
         using (DataTable table = _connection.GetSchema("Tables", new[] { null, null, SqlTableCommandDictionary[sqlTable].TableName, null }))
         {
            return table.Rows.Count != 0;
         }
      }

      internal void CreateTable(SqlTable sqlTable)
      {
         using (var command = SqlTableCommandDictionary[sqlTable].GetCreateTableCommand(_connection))
         {
            command.ExecuteNonQuery();
         }
      }

      internal void DropTable(SqlTable sqlTable)
      {
         using (var command = SqlTableCommandDictionary[sqlTable].GetDropTableCommand(_connection))
         {
            command.ExecuteNonQuery();
         }
      }

      internal string GetDatabaseVersion()
      {
         if (!TableExists(SqlTable.Version))
         {
            return null;
         }

         using (var adapter = new SQLiteDataAdapter("SELECT * FROM [DbVersion] ORDER BY ID DESC LIMIT 1;", _connection))
         using (var table = new DataTable())
         {
            adapter.Fill(table);
            if (table.Rows.Count != 0)
            {
               return table.Rows[0]["Version"].ToString();
            }
         }

         return null;
      }

      #endregion
      
      #endregion

      #region Private Helper Classes

      private static class WhereBuilder
      {
         public static PetaPoco.Sql Execute(QueryParameters parameters)
         {
            if (parameters.Fields.Count == 0)
            {
               return null;
            }

            // reset
            bool appendAnd = false;

            PetaPoco.Sql sql = PetaPoco.Sql.Builder.Append("WHERE ");
            foreach (var field in parameters.Fields)
            {
               if (field.Name.Equals(QueryFieldName.ID) ||
                   field.Name.Equals(QueryFieldName.ProjectID) ||
                   field.Name.Equals(QueryFieldName.ProjectRun) ||
                   field.Name.Equals(QueryFieldName.ProjectClone) ||
                   field.Name.Equals(QueryFieldName.ProjectGen) ||
                   field.Name.Equals(QueryFieldName.Name) ||
                   field.Name.Equals(QueryFieldName.Path) ||
                   field.Name.Equals(QueryFieldName.Username) ||
                   field.Name.Equals(QueryFieldName.Team) ||
                   field.Name.Equals(QueryFieldName.CoreVersion) ||
                   field.Name.Equals(QueryFieldName.FramesCompleted) ||
                   field.Name.Equals(QueryFieldName.FrameTime) ||
                   field.Name.Equals(QueryFieldName.Result) ||
                   field.Name.Equals(QueryFieldName.DownloadDateTime) ||
                   field.Name.Equals(QueryFieldName.CompletionDateTime))
               {
                  sql = sql.Append(appendAnd ? "AND " : String.Empty);
                  sql = BuildWhereCondition(sql, field);
                  appendAnd = true;
               }
            }
            
            return appendAnd ? sql.Append(" ORDER BY [ID] ASC") : null;
         }

         private static PetaPoco.Sql BuildWhereCondition(PetaPoco.Sql sql, QueryField queryField)
         {
            string format = "[{0}] {1} @0";
            if (queryField.Name.Equals(QueryFieldName.DownloadDateTime) ||
                queryField.Name.Equals(QueryFieldName.CompletionDateTime))
            {
               format = "datetime([{0}]) {1} datetime(@0)";
            }
            sql = sql.Append(String.Format(CultureInfo.InvariantCulture, format,
                     ColumnNameOverides.ContainsKey(queryField.Name) ? ColumnNameOverides[queryField.Name] : queryField.Name.ToString(),
                     queryField.Operator), queryField.Value);
            return sql;
         }

         private static readonly Dictionary<QueryFieldName, string> ColumnNameOverides = new Dictionary<QueryFieldName, string>
         {
            { QueryFieldName.Name, "InstanceName" },   
            { QueryFieldName.Path, "InstancePath" },
         };
      }

      private abstract class SqlTableCommands
      {
         #region SQL Strings

         private const string DropTableSql = "DROP TABLE [{0}];";

         #endregion

         public abstract string TableName { get; }

         public abstract DbCommand GetCreateTableCommand(SQLiteConnection connection);

         public DbCommand GetDropTableCommand(SQLiteConnection connection)
         {
            return new SQLiteCommand(connection)
            {
               CommandText = String.Format(CultureInfo.InvariantCulture, DropTableSql, TableName)
            };
         }
      }

      private sealed class WuHistorySqlTableCommands : SqlTableCommands
      {
         #region SQL Constants

         private const string WuHistoryTableName = "WuHistory";

         private const string WuHistoryTableCreateSql = "CREATE TABLE [{0}] (" +
                                                        "[ID] INTEGER  NOT NULL PRIMARY KEY AUTOINCREMENT," +
                                                        "[ProjectID] INT  NOT NULL," +
                                                        "[ProjectRun] INT  NOT NULL," +
                                                        "[ProjectClone] INT  NOT NULL," +
                                                        "[ProjectGen] INT  NOT NULL," +
                                                        "[InstanceName] VARCHAR(60)  NOT NULL," +
                                                        "[InstancePath] VARCHAR(260)  NOT NULL," +
                                                        "[Username] VARCHAR(60)  NOT NULL," +
                                                        "[Team] INT  NOT NULL," +
                                                        "[CoreVersion] FLOAT  NOT NULL," +
                                                        "[FramesCompleted] INT  NOT NULL," +
                                                        "[FrameTime] INT  NOT NULL," +
                                                        "[Result] INT  NOT NULL," +
                                                        "[DownloadDateTime] DATETIME  NOT NULL," +
                                                        "[CompletionDateTime] DATETIME  NOT NULL," +
                                                        "[WorkUnitName] VARCHAR(30)," +
                                                        "[KFactor] FLOAT," +
                                                        "[Core] VARCHAR(20)," +
                                                        "[Frames] INT," +
                                                        "[Atoms] INT," +
                                                        "[SlotType] VARCHAR(20)," +
                                                        "[Credit] FLOAT," +
                                                        "[PreferredDays] FLOAT," +
                                                        "[MaximumDays] FLOAT" +
                                                        ");";

         

         #endregion

         public override string TableName
         {
            get { return WuHistoryTableName; }
         }

         public override DbCommand GetCreateTableCommand(SQLiteConnection connection)
         {
            return new SQLiteCommand(connection)
            {
               CommandText = String.Format(CultureInfo.InvariantCulture,
                                           WuHistoryTableCreateSql, WuHistoryTableName)
            };
         }
      }

      private sealed class VersionSqlTableCommands : SqlTableCommands
      {
         #region SQL Constants

         private const string VersionTableName = "DbVersion";

         private const string VersionTableCreateSql = "CREATE TABLE [{0}] (" +
                                                      "[ID] INTEGER  NOT NULL PRIMARY KEY AUTOINCREMENT," +
                                                      "[Version] VARCHAR(20)  NOT NULL);";

         #endregion

         public override string TableName
         {
            get { return VersionTableName; }
         }

         public override DbCommand GetCreateTableCommand(SQLiteConnection connection)
         {
            return new SQLiteCommand(connection)
            {
               CommandText = String.Format(CultureInfo.InvariantCulture,
                                           VersionTableCreateSql, VersionTableName)
            };
         }
      }

      // ReSharper disable InconsistentNaming
      private class SQLiteColumnAdder
      // ReSharper restore InconsistentNaming
      {
         private readonly List<DbCommand> _commands = new List<DbCommand>();

         public string TableName { get; set; }

         public SQLiteConnection Connection { get; set; }

         public void AddColumn(string name, string dataType)
         {
            if (TableName == null || Connection == null)
            {
               return;
            }

            _commands.Add(new SQLiteCommand(Connection)
                          {
                             CommandText = String.Format(CultureInfo.InvariantCulture,
                                              "ALTER TABLE [{0}] ADD COLUMN [{1}] {2}", TableName, name, dataType)
                          });
         }

         public void Execute()
         {
            foreach (var command in _commands)
            {
               command.ExecuteNonQuery();
            }
         }
      }

      #endregion
   }
}
