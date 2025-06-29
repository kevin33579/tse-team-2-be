using System;
using System.Collections.Generic;
using System.Data;                    // DataSet, DataRow, etc.
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using ScheduleApi.Models;                 // assumes you already have a Schedule class

namespace ScheduleApi.Data
{
    /* ---------- Interface ---------- */
    public interface IScheduleRepository
    {
        Task<List<Schedule>> GetAllAsync();
        Task<Schedule?>      GetByIdAsync(int id);
        Task<int>            CreateAsync(Schedule schedule);   // returns new Id
        Task<bool>           UpdateAsync(Schedule schedule);   // returns true if row updated
        Task<bool>           DeleteAsync(int id);              // returns true if row deleted
    }

    /* ---------- Implementation ---------- */
    public class ScheduleRepository : IScheduleRepository
    {
        private readonly string _connectionString;

        public ScheduleRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new ArgumentNullException("Connection string tidak ditemukan");
        }

        /* ----------- READ ALL ----------- */
        public async Task<List<Schedule>> GetAllAsync()
        {
            var schedules = new List<Schedule>();

            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            const string query = @"SELECT id, time FROM schedule";

            using var command = new MySqlCommand(query, connection);
            using var reader  = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                schedules.Add(new Schedule
                {
                    Id   = reader.GetInt32("id"),
                    Time = reader.GetDateTime("time")
                });
            }

            return schedules;
        }

        /* ----------- READ ONE ----------- */
        public async Task<Schedule?> GetByIdAsync(int id)
        {
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            const string query = @"SELECT id, time FROM schedule WHERE id = @id";

            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@id", id);

            using var reader = await command.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                return new Schedule
                {
                    Id   = reader.GetInt32("id"),
                    Time = reader.GetDateTime("time")
                };
            }

            return null;
        }

        /* ----------- CREATE ----------- */
        public async Task<int> CreateAsync(Schedule schedule)
        {
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            const string query = @"INSERT INTO schedule (time) VALUES (@time);";

            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@time", schedule.Time);

            await command.ExecuteNonQueryAsync();

            // MySqlCommand.LastInsertedId returns the autoincremented value
            return (int)command.LastInsertedId;
        }

        /* ----------- UPDATE ----------- */
        public async Task<bool> UpdateAsync(Schedule schedule)
        {
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            const string query = @"UPDATE schedule SET time = @time WHERE id = @id";

            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@time", schedule.Time);
            command.Parameters.AddWithValue("@id",   schedule.Id);

            int rows = await command.ExecuteNonQueryAsync();
            return rows > 0;
        }

        /* ----------- DELETE ----------- */
        public async Task<bool> DeleteAsync(int id)
        {
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            const string query = @"DELETE FROM schedule WHERE id = @id";

            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@id", id);

            int rows = await command.ExecuteNonQueryAsync();
            return rows > 0;
        }
    }
}
