using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;

namespace YrlmzTakipSistemi
{
    public class GenericRepository<T> where T : class
    {
        protected readonly SQLiteConnection _connection;
        protected readonly string _tableName;

        public GenericRepository(SQLiteConnection connection, string tableName)
        {
            _connection = connection;
            _tableName = tableName;
        }

        public void Insert(Dictionary<string, object> parameters)
        {
            string columns = string.Join(", ", parameters.Keys);
            string values = string.Join(", ", parameters.Keys.Select(k => $"@{k}"));

            string query = $"INSERT INTO {_tableName} ({columns}) VALUES ({values})";

            using (var command = new SQLiteCommand(query, _connection))
            {
                foreach (var param in parameters)
                {
                    command.Parameters.AddWithValue($"@{param.Key}", param.Value);
                }
                command.ExecuteNonQuery();
            }
        }

        public List<Dictionary<string, object>> GetAll()
        {
            var resultList = new List<Dictionary<string, object>>();

            string query = $"SELECT * FROM {_tableName}";

            using (var command = new SQLiteCommand(query, _connection))
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    var row = new Dictionary<string, object>();
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        row[reader.GetName(i)] = reader.GetValue(i);
                    }
                    resultList.Add(row);
                }
            }
            return resultList;
        }

        public void Update(int id, Dictionary<string, object> parameters)
        {
            string setClause = string.Join(", ", parameters.Keys.Select(k => $"{k} = @{k}"));

            string query = $"UPDATE {_tableName} SET {setClause} WHERE Id = @Id";

            using (var command = new SQLiteCommand(query, _connection))
            {
                foreach (var param in parameters)
                {
                    command.Parameters.AddWithValue($"@{param.Key}", param.Value);
                }
                command.Parameters.AddWithValue("@Id", id);
                command.ExecuteNonQuery();
            }
        }

        public void Delete(int id)
        {
            string query = $"DELETE FROM {_tableName} WHERE Id = @Id";

            using (var command = new SQLiteCommand(query, _connection))
            {
                command.Parameters.AddWithValue("@Id", id);
                command.ExecuteNonQuery();
            }
        }
    }

}
