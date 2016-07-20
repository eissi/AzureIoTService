SELECT
    *
INTO
    [tablestorage]
FROM
    [deviceinput]
    
with timecasted AS (
select Deviceid,cast(Starttime as DATETIME) as [Start Time], cast(devicetime as DATETIME) as [Device Time]
from [deviceinput]
),
elapsedtime AS (
select deviceid, [Start Time], [Device Time], datediff([Start Time],[Device Time]) as ElapsedTime
from timecasted
)

SELECT
    Deviceid,[Start Time] as StartTime, [Device Time] as DeviceTime, ElapsedTime
Into [RDBMS]
from elapsetime

SELECT
    Deviceid,[Start Time] as StartTime, [Device Time] as DeviceTime, ElapsedTime
Into PowerBI
from elapsetime

SELECT
    deviceid,avg(ElpasedTime) as [AverageDeliveryTime], count(*) as NoTransaction, system.timestamp as [TimeStamp]
INTO
    [RDBMS-average]
FROM
    elapsedtime
GROUP BY deviceid, tumblingwindow(minute,1)

SELECT
    avg(cpuusage) as [Average CPU Usage], system.timestamp as [Time]
INTO
    [total-cpu-average]
FROM
    [cpu-usage]
GROUP BY tumblingwindow(minute,1)