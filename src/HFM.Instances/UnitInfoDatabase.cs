﻿/*
 * HFM.NET - Work Unit History Database
 * Copyright (C) 2009-2011 Ryan Harlamert (harlam357)
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
using System.Text;

using HFM.Framework;
using HFM.Framework.DataTypes;

namespace HFM.Instances
{
   public class UnitInfoDatabase : IUnitInfoDatabase
   {
      public const string WuHistoryTableName = "WuHistory";

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
                                                     "[CompletionDateTime] DATETIME  NOT NULL);";

      private const string WuHistoryTableDropSql = "DROP TABLE [{0}];";                                                     

      private const string WuHistoryTableInsertSql = "INSERT INTO [{0}] ([ProjectID]," +
                                                                        "[ProjectRun]," +
                                                                        "[ProjectClone]," +
                                                                        "[ProjectGen]," +
                                                                        "[InstanceName]," +
                                                                        "[InstancePath]," +
                                                                        "[Username]," +
                                                                        "[Team]," +
                                                                        "[CoreVersion]," +
                                                                        "[FramesCompleted]," +
                                                                        "[FrameTime]," +
                                                                        "[Result]," +
                                                                        "[DownloadDateTime]," +
                                                                        "[CompletionDateTime]) VALUES(?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?);";

      public string DatabaseFilePath { get; set; }
      private readonly IProteinCollection _proteinCollection;
   
      public UnitInfoDatabase(IProteinCollection proteinCollection)
      {
         _proteinCollection = proteinCollection;
      }
      
      public bool TableExists(string tableName)
      {
         using (var con = new SQLiteConnection(@"Data Source=" + DatabaseFilePath))
         {
            con.Open();
            return TableExists(con, WuHistoryTableName);
         }
      }
      
      public void CreateTable(string tableName)
      {
         using (var con = new SQLiteConnection(@"Data Source=" + DatabaseFilePath))
         {
            con.Open();
            CreateTable(con, WuHistoryTableName);
         }
      }
      
      public void DeleteAllUnitInfoData()
      {
         using (var con = new SQLiteConnection(@"Data Source=" + DatabaseFilePath))
         {
            con.Open();
            if (TableExists(con, WuHistoryTableName))
            {
               DropTable(con, WuHistoryTableName);
            }
         }
      }
   
      public void WriteUnitInfo(IUnitInfoLogic unitInfoLogic)
      {
         // validate the UnitInfoLogic before opening the connection
         if (ValidateUnitInfo(unitInfoLogic.UnitInfoData) == false) return;

         using (var con = new SQLiteConnection(@"Data Source=" + DatabaseFilePath))
         {
            con.Open();
            if (TableExists(con, WuHistoryTableName))
            {
               // do upgrade
            }
            else
            {
               CreateTable(con, WuHistoryTableName);
               Debug.Assert(TableExists(con, WuHistoryTableName));
            }

            // ensure this unit is not written twice
            if (UnitInfoExists(con, unitInfoLogic) == false)
            {
               try
               {
                  WriteUnitInfoToDatabase(con, unitInfoLogic);
               }
               catch (SQLiteException ex)
               {
                  HfmTrace.WriteToHfmConsole(ex);
               }
            }
         }
      }
      
      public int DeleteUnitInfo(long id)
      {
         using (var con = new SQLiteConnection(@"Data Source=" + DatabaseFilePath))
         {
            con.Open();
            if (TableExists(con, WuHistoryTableName))
            {
               var parameters = new QueryParameters();
               parameters.Fields.Add(new QueryField { Name = QueryFieldName.ID, Type = QueryFieldType.Equal, Value = id });
               return DeleteRows(con, parameters);
            }

            return 0;
         }
      }
      
      public void ImportCompletedUnits(ICollection<HistoryEntry> entries)
      {
         using (var con = new SQLiteConnection(@"Data Source=" + DatabaseFilePath))
         {
            con.Open();
            if (TableExists(con, WuHistoryTableName))
            {
               // do upgrade
            }
            else
            {
               CreateTable(con, WuHistoryTableName);
               Debug.Assert(TableExists(con, WuHistoryTableName));
            }

            using (var trans = con.BeginTransaction())
            {
               foreach (var historyEntry in entries)
               {
                  WriteUnitInfoToDatabase(con, historyEntry);
               }
               trans.Commit();
            }
         }
      }
      
      private static bool TableExists(DbConnection con, string tableName)
      {
         DataTable table = con.GetSchema("Tables", new[] { null, null, tableName, null });
         return table.Rows.Count != 0;
      }

      private static void CreateTable(SQLiteConnection con, string tableName)
      {
         using (var command = new SQLiteCommand(con))
         {
            command.CommandText = String.Format(CultureInfo.InvariantCulture,
                                                WuHistoryTableCreateSql, tableName);
            command.ExecuteNonQuery();
         }
      }
      
      private static void DropTable(SQLiteConnection con, string tableName)
      {
         using (var command = new SQLiteCommand(con))
         {
            command.CommandText = String.Format(CultureInfo.InvariantCulture,
                                                WuHistoryTableDropSql, tableName);
            command.ExecuteNonQuery();
         }
      }
      
      private static bool ValidateUnitInfo(IUnitInfo unitInfo)
      {
         bool result = ValidateFinishedUnitInfo(unitInfo);
         if (!(result))
         {
            // Issue 233
            result = ValidateIncompleteUnitInfo(unitInfo);
         }
         return result;
      }
      
      private static bool ValidateFinishedUnitInfo(IUnitInfo unitInfo)
      {
         return unitInfo.ProjectIsUnknown() == false &&
                unitInfo.UnitResult.Equals(WorkUnitResult.FinishedUnit) &&
                unitInfo.DownloadTime.Equals(DateTime.MinValue) == false &&
                unitInfo.FinishedTime.Equals(DateTime.MinValue) == false;
      }
      
      private static bool ValidateIncompleteUnitInfo(IUnitInfo unitInfo)
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

      private static bool UnitInfoExists(SQLiteConnection con, IUnitInfoLogic unitInfoLogic)
      {
         var rows = ExecuteQueryUnitData(con, BuildUnitKeyQueryParameters(unitInfoLogic));
         return rows.Count != 0;
      }

      //private static bool UnitInfoExists(SQLiteConnection con, HistoryEntry entry)
      //{
      //   var rows = ExecuteQueryUnitData(con, BuildUnitKeyQueryParameters(entry));
      //   return rows.Length != 0;
      //}
      
      private static QueryParameters BuildUnitKeyQueryParameters(IUnitInfoLogic unitInfoLogic)
      {
         var parameters = new QueryParameters();
         parameters.Fields.Add(new QueryField { Name = QueryFieldName.ProjectID, Type = QueryFieldType.Equal, Value = unitInfoLogic.UnitInfoData.ProjectID });
         parameters.Fields.Add(new QueryField { Name = QueryFieldName.ProjectRun, Type = QueryFieldType.Equal, Value = unitInfoLogic.UnitInfoData.ProjectRun });
         parameters.Fields.Add(new QueryField { Name = QueryFieldName.ProjectClone, Type = QueryFieldType.Equal, Value = unitInfoLogic.UnitInfoData.ProjectClone });
         parameters.Fields.Add(new QueryField { Name = QueryFieldName.ProjectGen, Type = QueryFieldType.Equal, Value = unitInfoLogic.UnitInfoData.ProjectGen });
         parameters.Fields.Add(new QueryField { Name = QueryFieldName.DownloadDateTime, Type = QueryFieldType.Equal, Value = unitInfoLogic.UnitInfoData.DownloadTime });
         return parameters;
      }

      private static void WriteUnitInfoToDatabase(SQLiteConnection con, IUnitInfoLogic unitInfoLogic)
      {
         using (var command = new SQLiteCommand(con))
         {
            HfmTrace.WriteToHfmConsole(TraceLevel.Info, unitInfoLogic.UnitInfoData.OwningInstanceName,
               String.Format(CultureInfo.CurrentCulture, "Writing unit {0} to database.", unitInfoLogic.UnitInfoData.ProjectRunCloneGen()));

            var projectID = new SQLiteParameter("ProjectID", DbType.Int32) { Value = unitInfoLogic.UnitInfoData.ProjectID };
            command.Parameters.Add(projectID);
            var projectRun = new SQLiteParameter("ProjectRun", DbType.Int32) { Value = unitInfoLogic.UnitInfoData.ProjectRun };
            command.Parameters.Add(projectRun);
            var projectClone = new SQLiteParameter("ProjectClone", DbType.Int32) { Value = unitInfoLogic.UnitInfoData.ProjectClone };
            command.Parameters.Add(projectClone);
            var projectGen = new SQLiteParameter("ProjectGen", DbType.Int32) { Value = unitInfoLogic.UnitInfoData.ProjectGen };
            command.Parameters.Add(projectGen);
            var instanceName = new SQLiteParameter("InstanceName", DbType.String) { Value = unitInfoLogic.UnitInfoData.OwningInstanceName };
            command.Parameters.Add(instanceName);
            var instancePath = new SQLiteParameter("InstancePath", DbType.String) { Value = unitInfoLogic.UnitInfoData.OwningInstancePath };
            command.Parameters.Add(instancePath);
            var username = new SQLiteParameter("Username", DbType.String) { Value = unitInfoLogic.UnitInfoData.FoldingID };
            command.Parameters.Add(username);
            var team = new SQLiteParameter("Team", DbType.Int32) { Value = unitInfoLogic.UnitInfoData.Team };
            command.Parameters.Add(team);
            var coreVersion = new SQLiteParameter("CoreVersion", DbType.Single) { Value = unitInfoLogic.UnitInfoData.CoreVersion };
            command.Parameters.Add(coreVersion);
            var framesCompleted = new SQLiteParameter("FramesCompleted", DbType.Int32) { Value = unitInfoLogic.FramesComplete };
            command.Parameters.Add(framesCompleted);
            var frameTime = new SQLiteParameter("FrameTime", DbType.Int32) { Value = unitInfoLogic.RawTimePerAllSections };
            command.Parameters.Add(frameTime);
            var result = new SQLiteParameter("Result", DbType.Int32) { Value = (int)unitInfoLogic.UnitInfoData.UnitResult };
            command.Parameters.Add(result);
            var downloadDateTime = new SQLiteParameter("DownloadDateTime", DbType.DateTime) { Value = unitInfoLogic.UnitInfoData.DownloadTime };
            command.Parameters.Add(downloadDateTime);
            var completionDateTime = new SQLiteParameter("CompletionDateTime", DbType.DateTime) { Value = unitInfoLogic.UnitInfoData.FinishedTime };
            command.Parameters.Add(completionDateTime);
            
            command.CommandText = String.Format(CultureInfo.InvariantCulture, WuHistoryTableInsertSql, WuHistoryTableName);
            command.ExecuteNonQuery();
         }
      }
      
      private static void WriteUnitInfoToDatabase(SQLiteConnection con, HistoryEntry entry)
      {
         using (var command = new SQLiteCommand(con))
         {
            var projectID = new SQLiteParameter("ProjectID", DbType.Int32) { Value = entry.ProjectID };
            command.Parameters.Add(projectID);
            var projectRun = new SQLiteParameter("ProjectRun", DbType.Int32) { Value = entry.ProjectRun };
            command.Parameters.Add(projectRun);
            var projectClone = new SQLiteParameter("ProjectClone", DbType.Int32) { Value = entry.ProjectClone };
            command.Parameters.Add(projectClone);
            var projectGen = new SQLiteParameter("ProjectGen", DbType.Int32) { Value = entry.ProjectGen };
            command.Parameters.Add(projectGen);
            var instanceName = new SQLiteParameter("InstanceName", DbType.String) { Value = entry.InstanceName };
            command.Parameters.Add(instanceName);
            var instancePath = new SQLiteParameter("InstancePath", DbType.String) { Value = entry.InstancePath };
            command.Parameters.Add(instancePath);
            var username = new SQLiteParameter("Username", DbType.String) { Value = entry.Username };
            command.Parameters.Add(username);
            var team = new SQLiteParameter("Team", DbType.Int32) { Value = entry.Team };
            command.Parameters.Add(team);
            var coreVersion = new SQLiteParameter("CoreVersion", DbType.Single) { Value = entry.CoreVersion };
            command.Parameters.Add(coreVersion);
            var framesCompleted = new SQLiteParameter("FramesCompleted", DbType.Int32) { Value = entry.FramesCompleted };
            command.Parameters.Add(framesCompleted);
            var frameTime = new SQLiteParameter("FrameTime", DbType.Int32) { Value = entry.FrameTime.TotalSeconds };
            command.Parameters.Add(frameTime);
            var result = new SQLiteParameter("Result", DbType.Int32) { Value = (int)entry.Result };
            command.Parameters.Add(result);
            var downloadDateTime = new SQLiteParameter("DownloadDateTime", DbType.DateTime) { Value = entry.DownloadDateTime };
            command.Parameters.Add(downloadDateTime);
            var completionDateTime = new SQLiteParameter("CompletionDateTime", DbType.DateTime) { Value = entry.CompletionDateTime };
            command.Parameters.Add(completionDateTime);

            command.CommandText = String.Format(CultureInfo.InvariantCulture, WuHistoryTableInsertSql, WuHistoryTableName);
            command.ExecuteNonQuery();
         }
      }

      public IList<HistoryEntry> QueryUnitData(QueryParameters parameters)
      {
         return QueryUnitData(parameters, HistoryProductionView.BonusDownloadTime);
      }
      
      public IList<HistoryEntry> QueryUnitData(QueryParameters parameters, HistoryProductionView productionView)
      {
         using (var con = new SQLiteConnection(@"Data Source=" + DatabaseFilePath))
         {
            con.Open();
            return ExecuteQueryUnitData(con, parameters, productionView, _proteinCollection);
         }
      }
      
      private static IList<HistoryEntry> ExecuteQueryUnitData(SQLiteConnection con, QueryParameters parameters)
      {
         return ExecuteQueryUnitData(con, parameters, HistoryProductionView.BonusDownloadTime, null);
      }

      private static IList<HistoryEntry> ExecuteQueryUnitData(SQLiteConnection con, QueryParameters parameters, 
                                                              HistoryProductionView productionView, IProteinCollection proteinCollection)
      {
         if (TableExists(con, WuHistoryTableName))
         {
            var table = GetDataTable(con, parameters);

            var query =
               from wu in table.AsEnumerable()
               select new HistoryEntry
               {
                  ID = wu.Field<long>("ID"),
                  ProjectID = wu.Field<int>("ProjectID"),
                  ProjectRun = wu.Field<int>("ProjectRun"),
                  ProjectClone = wu.Field<int>("ProjectClone"),
                  ProjectGen = wu.Field<int>("ProjectGen"),
                  InstanceName = wu.Field<string>("InstanceName"),
                  InstancePath = wu.Field<string>("InstancePath"),
                  Username = wu.Field<string>("Username"),
                  Team = wu.Field<int>("Team"),
                  CoreVersion = (float)wu.Field<double>("CoreVersion"),
                  FramesCompleted = wu.Field<int>("FramesCompleted"),
                  FrameTime = TimeSpan.FromSeconds(wu.Field<int>("FrameTime")),
                  Result = (WorkUnitResult)wu.Field<int>("Result"),
                  DownloadDateTime = DateTime.SpecifyKind(wu.Field<DateTime>("DownloadDateTime"), DateTimeKind.Utc),
                  CompletionDateTime = DateTime.SpecifyKind(wu.Field<DateTime>("CompletionDateTime"), DateTimeKind.Utc),
                  ProductionView = productionView
               };

            if (proteinCollection == null) return query.ToList();
            
            var proteins = proteinCollection.Proteins;
            var joinQuery = from entry in query
                            join protein in proteins on entry.ProjectID equals protein.ProjectNumber into groupJoin
                            from entryProtein in groupJoin.DefaultIfEmpty()
                            select entry.SetProtein(entryProtein);

            return FilterProteinParameters(parameters, joinQuery);
         }

         return new List<HistoryEntry>();
      }
      
      private static DataTable GetDataTable(SQLiteConnection con, QueryParameters parameters)
      {
         var selectBuilder = new SelectStatementBuilder();
         var command = new SQLiteCommand(selectBuilder.BuildSelectStatement(parameters), con);
         var adapter = new SQLiteDataAdapter(command);

         var table = new DataTable();
         adapter.Fill(table);
         return table;
      }

      private static int DeleteRows(SQLiteConnection con, QueryParameters parameters)
      {
         var selectBuilder = new SelectStatementBuilder();
         var command = new SQLiteCommand(selectBuilder.BuildSelectStatement(parameters), con);
         var adapter = new SQLiteDataAdapter(command);
         var table = new DataTable();
         adapter.Fill(table);

         if (table.Rows.Count != 0)
         {
            foreach (DataRow row in table.Rows)
            {
               row.Delete();
            }
            var builder = new SQLiteCommandBuilder(adapter);
            adapter.UpdateCommand = builder.GetUpdateCommand();
            return adapter.Update(table);
         }

         return 0;
      }
      
      private static IList<HistoryEntry> FilterProteinParameters(QueryParameters parameters, IEnumerable<HistoryEntry> entries)
      {
         var query = entries.AsQueryable();

         foreach (var field in parameters.Fields)
         {
            if (field.Name.Equals(QueryFieldName.WorkUnitName) ||
                field.Name.Equals(QueryFieldName.KFactor) ||
                field.Name.Equals(QueryFieldName.Core) ||
                field.Name.Equals(QueryFieldName.Frames) ||
                field.Name.Equals(QueryFieldName.Atoms) ||
                field.Name.Equals(QueryFieldName.ClientType) ||
                field.Name.Equals(QueryFieldName.PPD) ||
                field.Name.Equals(QueryFieldName.Credit))
            {
               try
               {
                  query = query.Where(BuildWhereCondition(field));
               }
               catch (ParseException ex)
               {
                  HfmTrace.WriteToHfmConsole(TraceLevel.Warning, ex);
               }
            }
         }

         return query.ToList();
      }

      private static string BuildWhereCondition(QueryField queryField)
      {
         string valueFormat = "{2}";
         if (queryField.Name.Equals(QueryFieldName.WorkUnitName) ||
             queryField.Name.Equals(QueryFieldName.Core))
         {
            valueFormat = "\"{2}\"";
         }
      
         var sbWhere = new StringBuilder();
         //if (queryField.Type.Equals(QueryFieldType.All) == false)
         //{
            sbWhere.AppendFormat(CultureInfo.InvariantCulture, "{0} {1} " + valueFormat, queryField.Name, queryField.Operator, queryField.Value);
         //}

         return sbWhere.ToString();
      }
   }

   internal class SelectStatementBuilder
   {
      private const string WuHistoryTableSelectAllSql = "SELECT * FROM [{0}] ";
      private const string AndSpace = "AND ";
   
      private bool _appendAnd;

      public string BuildSelectStatement(QueryParameters parameters)
      {
         if (parameters.Fields.Count == 0)
         {
            return String.Format(CultureInfo.InvariantCulture, WuHistoryTableSelectAllSql, UnitInfoDatabase.WuHistoryTableName);
         }
         
         // reset
         _appendAnd = false;

         var sbWhere = new StringBuilder("WHERE ");

         foreach (var field in parameters.Fields)
         {
            if (field.Name.Equals(QueryFieldName.ID) ||
                field.Name.Equals(QueryFieldName.ProjectID) ||
                field.Name.Equals(QueryFieldName.ProjectRun) ||
                field.Name.Equals(QueryFieldName.ProjectClone) ||
                field.Name.Equals(QueryFieldName.ProjectGen) ||
                field.Name.Equals(QueryFieldName.InstanceName) ||
                field.Name.Equals(QueryFieldName.InstancePath) ||
                field.Name.Equals(QueryFieldName.Username) ||
                field.Name.Equals(QueryFieldName.Team) ||
                field.Name.Equals(QueryFieldName.CoreVersion) ||
                field.Name.Equals(QueryFieldName.FramesCompleted) ||
                field.Name.Equals(QueryFieldName.FrameTime) ||
                field.Name.Equals(QueryFieldName.Result) ||
                field.Name.Equals(QueryFieldName.DownloadDateTime) ||
                field.Name.Equals(QueryFieldName.CompletionDateTime))
            {
               sbWhere.Append(BuildWhereCondition(field));
               sbWhere.Append(_appendAnd ? AndSpace : String.Empty);
            }
         }

         string selectCommand = String.Format(CultureInfo.InvariantCulture,
            WuHistoryTableSelectAllSql, UnitInfoDatabase.WuHistoryTableName);
         if (_appendAnd)
         {
            selectCommand += sbWhere.ToString();
            selectCommand = selectCommand.Remove(selectCommand.LastIndexOf(AndSpace)).Trim();
         }
         selectCommand += " ORDER BY [ID] ASC";

         return selectCommand;
      }

      private string BuildWhereCondition(QueryField queryField)
      {
         _appendAnd = true;

         var sbWhere = new StringBuilder();
         sbWhere.AppendFormat(CultureInfo.InvariantCulture, "[{0}] ", queryField.Name);
         sbWhere.Append(BuildValueCondition(queryField.Operator, queryField.Value));

         return sbWhere.ToString();
      }

      private static string BuildValueCondition(string oper, object value)
      {
         return String.Format(CultureInfo.InvariantCulture, "{0} {1} ", oper, GetFormattedValue(value));
      }

      private static string GetFormattedValue(object value)
      {
         if (value is DateTime)
         {
            var dateTime = (DateTime)value;
            return String.Format(CultureInfo.InvariantCulture, "'{0}'", dateTime.ToString("yyyy-MM-dd HH:mm:ss"));
         }

         return String.Format(CultureInfo.InvariantCulture, "'{0}'", value);
      }
   }
}