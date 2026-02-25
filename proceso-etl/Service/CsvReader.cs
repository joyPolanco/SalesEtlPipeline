using CsvHelper;
using System;
using System.Collections.Generic;
using System.Formats.Asn1;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace proceso_etl.Service
{
    public class CsvReader <TEntity>
    {

        public IEnumerable<TEntity> Extract(string _filePath)
        {
            using var reader = new StreamReader(_filePath);
            using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

            foreach (var record in csv.GetRecords<TEntity>())
            {
                yield return record; 
            }

        }
    }
}
