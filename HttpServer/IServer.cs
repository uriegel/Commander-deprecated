using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HttpServer
{
    public interface IServer
    {
        Configuration Configuration { get; }
    }
}
