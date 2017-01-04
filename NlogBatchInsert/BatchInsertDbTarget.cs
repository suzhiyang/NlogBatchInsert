using NLog;
using NLog.Common;
using NLog.Config;
using NLog.Targets;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NlogBatchInsert
{
    [Target("BatchInsertDbTarget")]
    public sealed class BatchInsertDbTarget : Target
    {
        [RequiredParameter]
        public string ConnectionString { get; set; }
        [RequiredParameter]
        public string TableName { get; set; }

        [ArrayParameter(typeof(DatabaseParameterInfo), "parameter")]
        public IList<DatabaseParameterInfo> Parameters { get; private set; }

        public BatchInsertDbTarget()
        {
            this.Parameters = new List<DatabaseParameterInfo>();
        }
        protected override void Write(AsyncLogEventInfo[] logEvents)
        {
            //Console.WriteLine("Batch processing: " + logEvents.Count() + " t" + Thread.CurrentThread.ManagedThreadId);

            List<LogEventInfo> events = logEvents.Select(x => x.LogEvent).ToList();
            // Do bulk insert in a background thread, always use sync methods in this function
            Task.Run(() => BulkInsert(events));

            //Console.WriteLine("Batch processing done---------------------------------------------");
        }

        public void BulkInsert(List<LogEventInfo> logEvents)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(this.ConnectionString))
                {
                    connection.Open();
                    DataTable dataTable = GenerateDataTable(logEvents, this.Parameters as List<DatabaseParameterInfo>);
                    using (SqlBulkCopy bulkCopy = new SqlBulkCopy(connection))
                    {
                        bulkCopy.DestinationTableName = this.TableName;
                        bulkCopy.WriteToServer(dataTable);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private DataTable GenerateDataTable(List<LogEventInfo> logEvents, List<DatabaseParameterInfo> parameterList)
        {
            DataTable logTable = new DataTable("BatchLogs");
            foreach (var parameter in parameterList)
            {
                // The column type can be omitted
                DataColumn column = new DataColumn();
                column.ColumnName = parameter.Name.Replace("@", "");
                logTable.Columns.Add(column);
            }

            // The parameters' order here MUST be the same with the sql schema in the config!!!
            foreach (var log in logEvents)
            {
                DataRow row = logTable.NewRow();
                foreach (var parameter in parameterList)
                {
                    row[parameter.Name.Replace("@", "")] = parameter.Layout.Render(log);
                }
                logTable.Rows.Add(row);
            }
            logTable.AcceptChanges();

            return logTable;
        }
    }
}
