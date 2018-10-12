using StandardCollections.Properties;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace StandardCollections
{
    internal static class Thrower
    {
        internal static void ArgumentException(ArgumentType paramName, string message)
        {
            if (paramName != ArgumentType.empty)
            {
                throw new ArgumentException(message, GetArgumentString(paramName));
            }
            throw new ArgumentException(message);
        }

        internal static void ArgumentNullException(ArgumentType paramName)
        {
            throw new ArgumentNullException(GetArgumentString(paramName));
        }
        internal static void ArgumentOutOfRangeException(ArgumentType paramName, string message)
        {
            if (paramName != ArgumentType.empty)
            {
                throw new ArgumentOutOfRangeException(GetArgumentString(paramName), message);
            }
            if (String.IsNullOrEmpty(message))
            {
                throw new ArgumentOutOfRangeException(GetArgumentString(paramName));
            }
            throw new ArgumentOutOfRangeException(message, (Exception)null);
        }
        internal static void InvalidOperationException(string message)
        {
            throw new InvalidOperationException(message, (Exception)null);
        }
        internal static void InvalidCastException(string message)
        {
            if (string.IsNullOrEmpty(message))
            {
                throw new InvalidCastException();
            }
            throw new InvalidCastException(message);
        }
        internal static void SerializationException(string message)
        {
            throw new SerializationException(message);
        }
        internal static void KeyNotFoundException(string message)
        {
            throw new KeyNotFoundException(message);
        }
        internal static void WrongValueTypeArgumentException(object value, Type type)
        {
            ArgumentException(ArgumentType.empty, string.Format(Resources.Argument_WrongType_G, new[] { value, type }));
        }

        internal static void NotSupportedException()
        {
            throw new NotSupportedException();
        }
        /// <summary>
        /// Thrown when enumerator and underlying collection does not share 'VERSION' field.
        /// </summary>
        internal static void EnumeratorCorrupted()
        {
            Thrower.InvalidOperationException(Resources.InvalidOperation_EnumCorrupt);
        }

        internal static string GetArgumentString(ArgumentType arg)
        {
            switch (arg)
            {
                case ArgumentType.array: return "array";
                case ArgumentType.arrayIndex: return "arrayIndex";
                case ArgumentType.capacity: return "capacity";
                case ArgumentType.collection: return "collection";
                case ArgumentType.columnIndex: return "columnIndex";
                case ArgumentType.comparer: return "comparer";
                case ArgumentType.converter: return "converter";
                case ArgumentType.count: return "count";
                case ArgumentType.data: return "data";
                case ArgumentType.dictionary: return "dictionary";
                case ArgumentType.elementSelector: return "elementSelector";
                case ArgumentType.info: return "info";
                case ArgumentType.index: return "index";
                case ArgumentType.item: return "item";
                case ArgumentType.key: return "key";
                case ArgumentType.key1: return "key1";
                case ArgumentType.key2: return "key2";
                case ArgumentType.keySelector: return "keySelector";
                case ArgumentType.left: return "left";
                case ArgumentType.list: return "list";
                case ArgumentType.lookup: return "lookup";
                case ArgumentType.lowerValue: return "lowerValue";
                case ArgumentType.match: return "match";
                case ArgumentType.newDimension: return "newDimension";
                case ArgumentType.newNode: return "newNode";
                case ArgumentType.node: return "node";
                case ArgumentType.other: return "other";
                case ArgumentType.right: return "right";
                case ArgumentType.rowIndex: return "rowIndex";
                case ArgumentType.size: return "size";
                case ArgumentType.startIndex: return "startIndex";
                case ArgumentType.source: return "source";
                case ArgumentType.target: return "target";
                case ArgumentType.upperValue: return "upperValue";
                case ArgumentType.value: return "value";
                case ArgumentType.vector: return "vector";
                case ArgumentType.weight: return "weight";
                default: return string.Empty;
            }
        }
    }
    internal enum ArgumentType
    {
        array,
        arrayIndex,
        capacity,
        collection,
        columnIndex,
        comparer,
        converter,
        count,
        data,
        dictionary,
        elementSelector,
        empty,
        info,
        index,
        item,
        key,
        key1,
        key2,
        keySelector,
        left,
        list,
        lookup,
        lowerValue,
        match,
        matrix,
        newDimension,
        newNode,
        node,
        other,
        right,
        rowIndex,
        size,
        startIndex,
        source,
        target,
        upperValue,
        value,
        vector,
        weight,
    }
}
