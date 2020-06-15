using Android.Util;
using Java.Lang;
using Java.Lang.Reflect;
using System.Collections.Generic;
using System.Linq;
using Object = Java.Lang.Object;

namespace Cab360Driver.Utils
{
    public class ReflectUtils
    {
        public static Class[] EMPTY_PARAM_TYPES = new Class[0];
        public static Object[] EMPTY_PARAMS = new Object[0];

        public static Field GetField(Class sourceClass, string fieldName, bool isDeclaredField, bool isUpwardFind)
        {
            Field field = null;
            try
            {
                field = isDeclaredField ? sourceClass.GetDeclaredField(fieldName) : sourceClass.GetField(fieldName);
            }
            catch (NoSuchFieldException nsfe)
            {
                Log.Debug("error", nsfe.Message);
                if (isUpwardFind)
                {
                    Class klass = sourceClass.Superclass;
                    while (field == null && klass != null)
                    {
                        try
                        {
                            field = isDeclaredField ? klass.GetDeclaredField(fieldName) : klass.GetField(fieldName);
                        }
                        catch (NoSuchFieldException nsfe2)
                        {
                            Log.Debug("error", nsfe2.Message);
                            klass = klass.Superclass;
                        }
                    }
                }
            }
            return field;
        }

        public static Field GetField(Class sourceClass, string fieldName)
        {
            return GetField(sourceClass, fieldName, true, true);
        }

        public static List<Field> GetFields(Class sourceClass, bool isGetDeclaredField, bool isGetParentField, bool isGetAllParentField, bool isDESCGet)
        {
            List<Field> fieldList = new List<Field>();
            if (isGetParentField)
            {
                List<Class> classList = null;
                if (isGetAllParentField)
                {
                    classList = GetSuperClasss(sourceClass, true);
                }
                else
                {
                    classList = new List<Class>(2);
                    classList.Add(sourceClass);
                    Class superClass = sourceClass.Superclass;
                    if (superClass != null)
                    {
                        classList.Add(superClass);
                    }
                }

                if (isDESCGet)
                {
                    for (int w = classList.Count() - 1; w > -1; w--)
                    {
                        foreach (Field field in isGetDeclaredField ? classList[w].GetDeclaredFields() : classList[w].GetFields())
                        {
                            fieldList.Add(field);
                        }
                    }
                }
                else
                {
                    for (int w = 0; w < classList.Count(); w++)
                    {
                        foreach (Field field in isGetDeclaredField ? classList[w].GetDeclaredFields() : classList[w].GetFields())
                        {
                            fieldList.Add(field);
                        }
                    }
                }
            }
            else
            {
                foreach (Field field in isGetDeclaredField ? sourceClass.GetDeclaredFields() : sourceClass.GetFields())
                {
                    fieldList.Add(field);
                }
            }
            return fieldList;
        }

        public static List<Field> GetFields(Class sourceClass)
        {
            return GetFields(sourceClass, true, true, true, true);
        }

        public static bool SetField(Object @object, string fieldName, Object newValue, bool isFindDeclaredField, bool isUpwardFind)
        {
            bool result = false;
            Field field = GetField(@object.Class, fieldName, isFindDeclaredField, isUpwardFind);
            if (field != null)
            {
                try
                {
                    field.Accessible = true;
                    field.Set(@object, newValue);
                    result = true;
                }
                catch (IllegalAccessException e)
                {
                    e.PrintStackTrace();
                    result = false;
                }
            }
            return result;
        }

        public static Method GetMethod(Class sourceClass, bool isFindDeclaredMethod, bool isUpwardFind, string methodName, Class[] methodParameterTypes)
        {
            Method method = null;
            try
            {
                method = isFindDeclaredMethod ? sourceClass.GetDeclaredMethod(methodName, methodParameterTypes) : sourceClass.GetMethod(methodName, methodParameterTypes);
            }
            catch (NoSuchMethodException e1)
            {
                Log.Debug("error", e1.Message);
                if (isUpwardFind)
                {
                    Class classs = sourceClass.Superclass;
                    while (method == null && classs != null)
                    {
                        try
                        {
                            method = isFindDeclaredMethod ? classs.GetDeclaredMethod(methodName, methodParameterTypes) : classs.GetMethod(methodName, methodParameterTypes);
                        }
                        catch (NoSuchMethodException e11)
                        {
                            Log.Debug("error", e11.Message);
                            classs = classs.Superclass;
                        }
                    }
                }
            }
            return method;
        }

        public static Method GetMethod(Class sourceClass, string methodName, Class[] methodParameterTypes)
        {
            return GetMethod(sourceClass, true, true, methodName, methodParameterTypes);
        }

        public static Method GetMethod(Class sourceClass, string methodName)
        {
            return GetMethod(sourceClass, methodName, EMPTY_PARAM_TYPES);
        }

        public static List<Method> GetMethods(Class clas, bool isGetDeclaredMethod, bool isFromSuperClassGet, bool isDESCGet)
        {
            List<Method> methodList = new List<Method>();
            if (isFromSuperClassGet)
            {
                List<Class> classList = GetSuperClasss(clas, true);
                if (isDESCGet)
                {
                    for (int w = classList.Count() - 1; w > -1; w--)
                    {
                        foreach (Method method in isGetDeclaredMethod ? classList[w].GetDeclaredMethods() : classList[w].GetMethods())
                        {
                            methodList.Add(method);
                        }
                    }
                }
                else
                {
                    for (int w = 0; w < classList.Count(); w++)
                    {
                        foreach (Method method in isGetDeclaredMethod ? classList[w].GetDeclaredMethods() : classList[w].GetMethods())
                        {
                            methodList.Add(method);
                        }
                    }
                }
            }
            else
            {
                foreach (Method method in isGetDeclaredMethod ? clas.GetDeclaredMethods() : clas.GetMethods())
                {
                    methodList.Add(method);
                }
            }
            return methodList;
        }

        public static List<Method> GetMethods(Class sourceClass)
        {
            return GetMethods(sourceClass, true, true, true);
        }

        public static Method GetValueOfMethod(Class sourceClass, Class[] methodParameterTypes)
        {
            return GetMethod(sourceClass, true, true, "valueOf", methodParameterTypes);
        }

        public static Object InvokeMethod(Method method, Object @object)
        {
            return method.Invoke(@object, EMPTY_PARAMS);
        }

        public static Constructor GetConstructor(Class sourceClass, bool isFindDeclaredConstructor, bool isUpwardFind, Class constructorParameterTypes)
        {
            Constructor method = null;
            try
            {
                method = isFindDeclaredConstructor ? sourceClass.GetDeclaredConstructor(constructorParameterTypes) : sourceClass.GetConstructor(constructorParameterTypes);
            }
            catch (NoSuchMethodException e1)
            {
                Log.Debug("error", e1.Message);
                if (isUpwardFind)
                {
                    Class classs = sourceClass.Superclass;
                    while (method == null && classs != null)
                    {
                        try
                        {
                            method = isFindDeclaredConstructor ? sourceClass.GetDeclaredConstructor(constructorParameterTypes) : sourceClass.GetConstructor(constructorParameterTypes);
                        }
                        catch (NoSuchMethodException e11)
                        {
                            Log.Debug("error", e11.Message);
                            classs = classs.Superclass;
                        }
                    }
                }
            }
            return method;
        }

        public static List<Constructor> GetConstructors(Class sourceClass, bool isFindDeclaredConstructor, bool isFromSuperClassGet, bool isDESCGet)
        {
            List<Constructor> constructorList = new List<Constructor>();
            if (isFromSuperClassGet)
            {
                List<Class> classList = GetSuperClasss(sourceClass, true);

                if (isDESCGet)
                {
                    for (int w = classList.Count() - 1; w > -1; w--)
                    {
                        foreach (Constructor constructor in isFindDeclaredConstructor ? classList[w].GetDeclaredConstructors() : classList[w].GetConstructors())
                        {
                            constructorList.Add(constructor);
                        }
                    }
                }
                else
                {
                    for (int w = 0; w < classList.Count(); w++)
                    {
                        foreach (Constructor constructor in isFindDeclaredConstructor ? classList[w].GetDeclaredConstructors() : classList[w].GetConstructors())
                        {
                            constructorList.Add(constructor);
                        }
                    }
                }
            }
            else
            {
                foreach (Constructor constructor in isFindDeclaredConstructor ? sourceClass.GetDeclaredConstructors() : sourceClass.GetConstructors())
                {
                    constructorList.Add(constructor);
                }
            }
            return constructorList;
        }

        public static List<Class> GetSuperClasss(Class sourceClass, bool isAddCurrentClass)
        {
            List<Class> classList = new List<Class>();
            Class classs;
            if (isAddCurrentClass)
            {
                classs = sourceClass;
            }
            else
            {
                classs = sourceClass.Superclass;
            }
            while (classs != null)
            {
                classList.Add(classs);
                classs = classs.Superclass;
            }
            return classList;
        }

        public static string GetClassName(Class sourceClass)
        {
            string classPath = sourceClass.Name;
            return classPath.Substring(classPath.LastIndexOf('.') + 1);
        }

        public static bool IsArrayByType(Field field, Class type)
        {
            Class fieldType = field.Type;
            return fieldType.IsArray && type.IsAssignableFrom(fieldType.ComponentType);
        }

    }
}