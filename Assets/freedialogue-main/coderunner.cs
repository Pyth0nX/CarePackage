using UnityEngine;
using UnityEngine.Events;
namespace OpenDialogue
{
    public class coderunner : MonoBehaviour
    {
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {

        }
        public func[] functions;
        public void runCode(string id)
        {
            foreach (func f in functions)
            {
                if (f.name == id)
                {
                    f.e.Invoke();
                }
            }
        }
        // Update is called once per frame
        void Update()
        {

        }
    }

    public class func
    {
        public UnityEvent e;
        public string name;
    }
}
