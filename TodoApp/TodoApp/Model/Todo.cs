using Microsoft.Azure.Cosmos.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TodoApp.Model
{
    public class Todo : TableEntity
    {
        public Todo()
        {
            PartitionKey = "Todos";
            RowKey = Guid.NewGuid().ToString("n");
        }

        public DateTime CreatedTime { get; set; } = DateTime.UtcNow;
        public string TaskDescription { get; set; }
        public bool IsCompleted { get; set; }
    }
    //public class Todo
    //{
    //    public string Id { get; set; } = Guid.NewGuid().ToString("n");
    //    public DateTime CreatedTime { get; set; } = DateTime.UtcNow;
    //    public string TaskDescription { get; set; }
    //    public bool IsCompleted { get; set; }

    //}
    public class TodoCreateModel
    {
        public string TaskDescription { get; set; }

    }
    public class TodoUpdateModel
    {
        public string TaskDescription { get; set; }
        public bool IsCompleted { get; set; }
    }
}



