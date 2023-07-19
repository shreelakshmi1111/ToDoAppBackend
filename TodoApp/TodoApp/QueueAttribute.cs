using System;

internal class QueueAttribute : Attribute
{
    private string v;
    private string connection;

    public QueueAttribute(string v, string Connection)
    {
        this.v = v;
        connection = Connection;
    }
}