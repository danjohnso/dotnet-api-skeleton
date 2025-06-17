using System.Reflection;

namespace Skeleton.Core
{
    public static class ProblemHelper
    {
        /// <summary>
        /// Builds full dictionary of Problems, including <see cref="Problems"/>, based on the namespace of a Type that contains Problems
        /// </summary>
        /// <param name="type"></param>
        public static SortedDictionary<string, string> BuildProblemCodeDictionary(Type type)
        {
            SortedDictionary<string, string> problemCodes = [];

            GetProblems(problemCodes, typeof(Problems));

            IEnumerable<Type> messageTypes = type.Assembly.GetTypes().Where(x => x.Namespace == type.Namespace);
            foreach (Type messageType in messageTypes)
            {
                GetProblems(problemCodes, messageType);
            }

            return problemCodes;
        }

        /// <summary>
        /// Used to build sorted problem dictionary from **FIELDS** on the provided type
        /// </summary>
        /// <param name="problemCodes"></param>
        /// <param name="type"></param>
        public static void GetProblems(SortedDictionary<string, string> problemCodes, Type type)
        {
            FieldInfo[] fields = type.GetFields(BindingFlags.Public | BindingFlags.Static);
            foreach (FieldInfo field in fields)
            {
                object? value = field.GetValue(type);
                if (value != null && value is Problem valueProblem)
                {
                    problemCodes.Add(valueProblem.Code, valueProblem.Message);
                }
            }
        }

        public static Problem Format(this Problem problem, params object?[] args)
        {
            return new Problem(problem.Code, string.Format(problem.Message, args));
        }
    }
}
