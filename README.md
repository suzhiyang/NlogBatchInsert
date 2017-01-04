# NlogBatchInsert Sample
A sample to optimize Nlog SQL database batch insert performance.

In this sample, we implement a BatchInsertDbTarget inherited from Target base class. The [Database target](https://github.com/NLog/NLog/wiki/Database-target) provided by Nlog performs insert operation one by one, even it is wrapped in a [BufferingWrapper target](https://github.com/NLog/NLog/wiki/BufferingWrapper-target). 

This implementation uses a dedicated thread to perform bulk insert, and will never block the calling thread.

## Getting Started
1. Create a log table using the following command (with a customized field traceid, which is a thread context):
```
    CREATE TABLE [dbo].[test] (
    [Id] [bigint] IDENTITY (1, 1) NOT NULL,
    [Traceid] [varchar] (50) NOT NULL,
    [Date] [datetime] NOT NULL,
    [Thread] [varchar] (255) NOT NULL,
    [Level] [varchar] (50) NOT NULL,
    [Message] [varchar] (5000) NULL,)
```

2. Edit NLog.config
   - Fill in the connection string and the table name
   - Uncomment the "asyncdb" rule
   
3. Register the BatchInsertDbTarget at the very beginning of your program. 
```Target.Register("BatchInsertDbTarget", typeof(BatchInsertDbTarget));```
Done!

## Notes
1. Customize your own fields by adding new parameter and modifying the table schema.
2. The parameters' order defined in the config should be EXACTLY the same as the table schema. Otherwise, nothing can be inserted to the log table.
3. By default, we enable a colored console target and a file target.
4. Change the buffer size and flush timeout parameter as needed.
5. Parameter to table schema mapping rule: "@variablename" -> "variablename", e.g. "@traceid" -> "traceid".
6. Sample output:

```
2017/01/04 18:37:43.541 [DEBUG] [t16] Trace 6 Message 0
2017/01/04 18:37:43.541 [DEBUG] [t11] Trace 1 Message 0
2017/01/04 18:37:43.541 [DEBUG] [t18] Trace 8 Message 0
2017/01/04 18:37:43.541 [DEBUG] [t15] Trace 5 Message 0
2017/01/04 18:37:43.541 [DEBUG] [t14] Trace 4 Message 0
2017/01/04 18:37:43.541 [DEBUG] [t13] Trace 3 Message 0
2017/01/04 18:37:43.541 [DEBUG] [t10] Trace 0 Message 0
2017/01/04 18:37:43.541 [DEBUG] [t17] Trace 7 Message 0
2017/01/04 18:37:43.541 [DEBUG] [t12] Trace 2 Message 0
2017/01/04 18:37:43.541 [DEBUG] [t19] Trace 9 Message 0
2017/01/04 18:37:43.556 [DEBUG] [t16] Trace 6 Message 1
2017/01/04 18:37:43.556 [DEBUG] [t14] Trace 4 Message 1
2017/01/04 18:37:43.556 [DEBUG] [t16] Trace 6 Message 2
2017/01/04 18:37:43.556 [DEBUG] [t16] Trace 6 Message 3
2017/01/04 18:37:43.556 [DEBUG] [t11] Trace 1 Message 1
2017/01/04 18:37:43.556 [DEBUG] [t11] Trace 1 Message 2
```