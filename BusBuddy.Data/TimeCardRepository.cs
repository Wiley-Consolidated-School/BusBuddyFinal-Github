using System;
using System.Collections.Generic;
using System.Data;
using BusBuddy.Models;
using Dapper;

namespace BusBuddy.Data
{
    public class TimeCardRepository : BaseRepository, ITimeCardRepository
    {
        public TimeCardRepository() : base()
        {
        }

        public List<TimeCard> GetAllTimeCards()
        {
            using (var connection = CreateConnection())
            {
                connection.Open();
                var timeCards = connection.Query<TimeCard>("SELECT * FROM TimeCard").AsList();
                return timeCards;
            }
        }

        public TimeCard GetTimeCardById(int id)
        {
            using (var connection = CreateConnection())
            {
                connection.Open();
                return connection.QuerySingleOrDefault<TimeCard>(
                    "SELECT * FROM TimeCard WHERE TimeCardID = @TimeCardID",
                    new { TimeCardID = id });
            }
        }

        public List<TimeCard> GetTimeCardsByDate(DateTime date)
        {
            using (var connection = CreateConnection())
            {
                connection.Open();
                var timeCards = connection.Query<TimeCard>(
                    "SELECT * FROM TimeCard WHERE Date = @Date",
                    new { Date = date }).AsList();
                return timeCards;
            }
        }

        public List<TimeCard> GetTimeCardsByDateRange(DateTime startDate, DateTime endDate)
        {
            using (var connection = CreateConnection())
            {
                connection.Open();
                var timeCards = connection.Query<TimeCard>(
                    "SELECT * FROM TimeCard WHERE Date BETWEEN @StartDate AND @EndDate",
                    new { StartDate = startDate, EndDate = endDate }).AsList();
                return timeCards;
            }
        }

        public List<TimeCard> GetTimeCardsByDayType(string dayType)
        {
            using (var connection = CreateConnection())
            {
                connection.Open();
                var timeCards = connection.Query<TimeCard>(
                    "SELECT * FROM TimeCard WHERE DayType = @DayType",
                    new { DayType = dayType }).AsList();
                return timeCards;
            }
        }

        public int AddTimeCard(TimeCard timeCard)
        {
            using (var connection = CreateConnection())
            {
                connection.Open();
                var sql = @"
                    INSERT INTO TimeCard (
                        Date, DayType,
                        AMClockIn, LunchClockOut, LunchClockIn, PMClockOut,
                        RouteAMClockOut, RouteAMClockIn, RoutePMClockOut, RoutePMClockIn,
                        TotalTime, Overtime, WeeklyTotal, MonthlyTotal
                    )
                    VALUES (
                        @Date, @DayType,
                        @AMClockIn, @LunchClockOut, @LunchClockIn, @PMClockOut,
                        @RouteAMClockOut, @RouteAMClockIn, @RoutePMClockOut, @RoutePMClockIn,
                        @TotalTime, @Overtime, @WeeklyTotal, @MonthlyTotal
                    );
                    SELECT last_insert_rowid();";

                return connection.QuerySingle<int>(sql, timeCard);
            }
        }

        public bool UpdateTimeCard(TimeCard timeCard)
        {
            using (var connection = CreateConnection())
            {
                connection.Open();
                var sql = @"
                    UPDATE TimeCard
                    SET Date = @Date,
                        DayType = @DayType,
                        AMClockIn = @AMClockIn,
                        LunchClockOut = @LunchClockOut,
                        LunchClockIn = @LunchClockIn,
                        PMClockOut = @PMClockOut,
                        RouteAMClockOut = @RouteAMClockOut,
                        RouteAMClockIn = @RouteAMClockIn,
                        RoutePMClockOut = @RoutePMClockOut,
                        RoutePMClockIn = @RoutePMClockIn,
                        TotalTime = @TotalTime,
                        Overtime = @Overtime,
                        WeeklyTotal = @WeeklyTotal,
                        MonthlyTotal = @MonthlyTotal
                    WHERE TimeCardID = @TimeCardID";

                var rowsAffected = connection.Execute(sql, timeCard);
                return rowsAffected > 0;
            }
        }

        public bool DeleteTimeCard(int id)
        {
            using (var connection = CreateConnection())
            {
                connection.Open();
                var sql = "DELETE FROM TimeCard WHERE TimeCardID = @TimeCardID";
                var rowsAffected = connection.Execute(sql, new { TimeCardID = id });
                return rowsAffected > 0;
            }
        }
    }
}
