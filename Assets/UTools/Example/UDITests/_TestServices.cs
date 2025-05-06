using UnityEngine;
using UnityEngine.Events;
using UTools;
namespace UTools.Example
{
    public class _TestServiceA
    {
        // -- field injection --
        [Inject]
        public _TestServiceB TestServiceB;

        // -- or property injection --
        // [Inject]
        // public _TestServiceB TestServiceB { get;private set; }

        // -- or method injection --
        // public _TestServiceB TestServiceB { get; private set; }
        // [Inject]
        // public void InjectTestServiceA(_TestServiceB testServiceB)
        // {
        //     TestServiceB = testServiceB;
        // }



        public string SayHello()
        {
            string str = "Hello from ServiceA";
            Debug.Log(str);
            return str;
        }
    }
    public class _TestServiceB
    {
        public string SayHello()
        {
            string str = "Hello from ServiceB";
            Debug.Log(str);
            return str;
        }
    }

    public class _TestServiceC
    {
        public string Message{get;private set;}
        [PostInjection]
        void SayHello()
        { 
            Message = "Hello from ServiceC in post injection";
            
        }
    }
}
