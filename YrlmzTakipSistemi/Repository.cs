using System.Data.SQLite;

namespace YrlmzTakipSistemi.Repositories
{
    public class Repository<T> : IRepository<T> where T : class
    {
        public readonly SQLiteConnection _connection;
        private readonly string _tableName;

        public Repository(SQLiteConnection connection, string tableName)
        {
            _connection = connection;
            _tableName = tableName;
        }

        public long Add(T entity)
        {
            var properties = typeof(T).GetProperties()
                .Where(p => p.Name != "Id" && p.Name != "Tarih" && p.Name != "AlacakDurumu" && p.Name != "KumulatifAlacak" && p.Name != "KategoriDescription" && p.Name != "RowNumber")
                .ToList();
            var columns = string.Join(", ", properties.Select(p => p.Name));
            var values = string.Join(", ", properties.Select(p => $"@{p.Name}"));

            var query = $"INSERT INTO {_tableName} ({columns}) VALUES ({values}); SELECT last_insert_rowid();";

            try
            {
                using (var command = new SQLiteCommand(query, _connection))
                {
                    _connection.Open();
                    foreach (var property in properties)
                    {
                        object value = property.GetValue(entity);

                        command.Parameters.AddWithValue($"@{property.Name}", value ?? DBNull.Value);
                    }
                    return (long)command.ExecuteScalar();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Veri eklenirken bir hata oluştu: " + ex.Message, ex);
            }
            finally
            {
                _connection.Close();
            }
        }

        public void Update(T entity)
        {
            var properties = typeof(T).GetProperties()
                .Where(p => p.Name != "AlacakDurumu" && p.Name != "KategoriDescription" && p.Name != "KumulatifAlacak" && p.Name != "RowNumber")
                .ToList();
            var setValues = string.Join(", ", properties.Select(p => $"{p.Name} = @{p.Name}"));

            var query = $"UPDATE {_tableName} SET {setValues} WHERE Id = @Id";

            try
            {
                _connection.Open();
                using (var command = new SQLiteCommand(query, _connection))
                {
                    foreach (var property in properties)
                    {
                        object value = property.GetValue(entity);

                        command.Parameters.AddWithValue($"@{property.Name}", value ?? DBNull.Value);
                    }
                    command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Veri güncellenirken bir hata oluştu.", ex);
            }
            finally
            {
                _connection.Close();
            }
        }

        public void Delete(int id)
        {
            var query = $"DELETE FROM {_tableName} WHERE Id = @Id";

            try
            {
                using (var command = new SQLiteCommand(query, _connection))
                {
                    _connection.Open();
                    command.Parameters.AddWithValue("@Id", id);
                    command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Veri silinirken bir hata oluştu.", ex);
            }
            finally
            {
                _connection.Close();
            }
        }

        public T GetById(int id)
        {
            var query = $"SELECT * FROM {_tableName} WHERE Id = @Id";

            try
            {
                using (var command = new SQLiteCommand(query, _connection))
                {
                    _connection.Open();
                    command.Parameters.AddWithValue("@Id", id);

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return MapToEntity(reader);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Veri alınırken bir hata oluştu.", ex);
            }
            finally
            {
                _connection.Close();
            }
            return null;
        }

        public List<T> GetAll(string filter = null)
        {
            var query = $"SELECT * FROM {_tableName}";
            if (!string.IsNullOrEmpty(filter))
            {
                query += $" WHERE {filter}";
            }

            var entities = new List<T>();
            try
            {
                using (var command = new SQLiteCommand(query, _connection))
                {
                    _connection.Open();
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            entities.Add(MapToEntity(reader));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Veriler alınırken bir hata oluştu.", ex);
            }
            finally
            {
                _connection.Close();
            }
            return entities;
        }

        public List<T> GetWithAValue(string value, string filter = null)
        {
            var query = $"SELECT {value} FROM {_tableName}";
            if (!string.IsNullOrEmpty(filter))
            {
                query += $" WHERE {filter}";
            }

            var entities = new List<T>();
            try
            {
                using (var command = new SQLiteCommand(query, _connection))
                {
                    _connection.Open();
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            entities.Add(MapToEntity(reader));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Veriler alınırken bir hata oluştu." + ex.Message, ex);
            }
            finally
            {
                _connection.Close();
            }
            return entities;
        }

        private T MapToEntity(SQLiteDataReader reader)
        {
            var entity = Activator.CreateInstance<T>();
            var properties = typeof(T).GetProperties();

            foreach (var property in properties)
            {
                if (!reader.IsDBNull(reader.GetOrdinal(property.Name)))
                {
                    var value = reader[property.Name];
                    property.SetValue(entity, ConvertValue(value, property.PropertyType));
                }
            }
            return entity;
        }

        private object ConvertValue(object value, Type targetType)
        {
            if (value == null || value == DBNull.Value)
            {
                return null;
            }

            if (targetType == typeof(int))
            {
                return Convert.ToInt32(value);
            }
            if (targetType == typeof(long))
            {
                return Convert.ToInt64(value);
            }
            if (targetType == typeof(double))
            {
                return Convert.ToDouble(value);
            }
            if (targetType == typeof(decimal))
            {
                return Convert.ToDecimal(value);
            }
            if (targetType == typeof(DateTime))
            {
                return Convert.ToDateTime(value);
            }
            if (targetType == typeof(string))
            {
                return value.ToString();
            }

            return value;
        }
    }
}