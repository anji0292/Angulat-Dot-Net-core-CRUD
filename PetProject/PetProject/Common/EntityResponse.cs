using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PetProject.Common
{

    public class EntityResponse<T> : Response
    {
        public T Data { get; set; }
    }

}
    