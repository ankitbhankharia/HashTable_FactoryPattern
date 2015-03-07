/*
 * Solution.cs
 * 
 * @version
 * $Id: Solution.cs, Version 1.0 02/09/2015
 * 
 * @revision
 * $Log initial version $
 * 
 */

/**
 * 
 * The Program implements hash table using factory pattern
 * 
 * @author Ankit Bhankharia (atb5880)
 * 
 */

using System;
using System.Collections.Generic;

namespace RIT_CS
{
    /// <summary>
    /// An exception used to indicate a problem with how
    /// a HashTable instance is being accessed
    /// </summary>
    public class NonExistentKey<Key> : Exception
    {
        /// <summary>
        /// The key that caused this exception to be raised
        /// </summary>
        public Key BadKey { get; private set; }

        /// <summary>
        /// Create a new instance and save the key that
        /// caused the problem.
        /// </summary>
        /// <param name="k">
        /// The key that was not found in the hash table
        /// </param>
        public NonExistentKey(Key k) :
            base("Non existent key in HashTable: " + k)
        {
            BadKey = k;
        }

    }

    /// <summary>
    /// An associative (key-value) data structure.
    /// A given key may not appear more than once in the table,
    /// but multiple keys may have the same value associated with them.
    /// Tables are assumed to be of limited size are expected to automatically
    /// expand if too many entries are put in them.
    /// </summary>
    /// <param name="Key">the types of the table's keys (uses Equals())</param>
    /// <param name="Value">the types of the table's values</param>
    interface Table<Key, Value> : IEnumerable<Key>
    {
        /// <summary>
        /// Add a new entry in the hash table. If an entry with the
        /// given key already exists, it is replaced without error.
        /// put() always succeeds.
        /// (Details left to implementing classes.)
        /// </summary>
        /// <param name="k">the key for the new or existing entry</param>
        /// <param name="v">the (new) value for the key</param>
        void Put(Key k, Value v);

        /// <summary>
        /// Does an entry with the given key exist?
        /// </summary>
        /// <param name="k">the key being sought</param>
        /// <returns>true iff the key exists in the table</returns>
        bool Contains(Key k);

        /// <summary>
        /// Fetch the value associated with the given key.
        /// </summary>
        /// <param name="k">The key to be looked up in the table</param>
        /// <returns>the value associated with the given key</returns>
        /// <exception cref="NonExistentKey">if Contains(key) is false</exception>
        Value Get(Key k);
    }

    /// <summary>
    /// Stores the key value pair for any given entry
    /// </summary>
    /// <param name="Key">the types of the table's keys</param>
    /// <param name="Value">the types of the table's values</param>
    public class KeyValuePair<Key, Value> {
        public Key K{get; set; }
        public Value V{get; set; }
    }

    /// <summary>
    /// An (key-value) data structure that implements Table interface.
    /// A given key will not appear more than once in the table,
    /// but multiple keys may have the same value associated with them.
    /// </summary>
    /// <param name="Key">the types of the keys (uses Equals())</param>
    /// <param name="Value">the types of the values</param>
    class LinkedHashedTable<Key, Value> : Table<Key, Value>
    {
      
        // counts the total key value pairs
        int counter = 0;
        // capacity value
        int maxSize;
        // Threshold value
        double threshold;
        // Stores the key value pair
        List<KeyValuePair<Key, Value>>[] list;

        /// <summary>
        /// Sets the intial capacity, threshold and create list objects
        /// </summary>
        /// <param name="max">capacity of our table</param>
        /// <param name="thresh">threshold</param>
        public LinkedHashedTable(int max, double thresh)
        {
            maxSize = max;
            threshold = thresh;
            list = new List<KeyValuePair<Key, Value>>[maxSize];
            for (int i = 0; i < list.Length; i++)
            {
                list[i] = new List<KeyValuePair<Key, Value>>();
            }
        }

        /// <summary>
        /// when the number of entries exceeds capacity, we exapnd our table by
        /// increasing the capacity of our table.
        /// </summary>
        public void Rehashing()
        {
            if (counter >= (maxSize * threshold))
            {
                // Increase the capacity by threshold value
                maxSize += Convert.ToInt32(maxSize * threshold);
                // Temporary list of nixe equal to new capacity
                List<KeyValuePair<Key, Value>>[] temporary = 
                new List<KeyValuePair<Key, Value>>[maxSize];

                for (int i = 0; i < temporary.Length; i++)
                {
                    temporary[i] = new List<KeyValuePair<Key, Value>>();
                }

                // Store the key value pairs from old list to new list
                // based on new hash code
                for (int i = 0; i < list.Length; i++)
                {
                    for (int j = 0; j < list[i].Count; j++)
                    {
                        Key tempKey = list[i][j].K;
                        Value tempVal = list[i][j].V;
                        int position = Math.Abs(tempKey.GetHashCode() % maxSize);
                        temporary[position].Add(new KeyValuePair<Key, Value>() 
                        { K = tempKey, V = tempVal });
                    }
                }
                // copy the key value pairs with new position back in old list
                list = temporary;
            }
        }

        /// <summary>
        /// Add a new entry in the hash table. If an entry with the
        /// given key already exists, it is replaced without error.
        /// put() always succeeds.
        /// </summary>
        /// <param name="k">the key for the new or existing entry</param>
        /// <param name="v">the (new) value for the key</param>
        public void Put(Key k, Value v)
        {
            // checks if the new key is already present
            bool duplicateFound = false;
            //Creates an object of KeyValuePair with supplied key and value
            KeyValuePair<Key, Value> current = new KeyValuePair<Key, Value>() 
            { K = k, V = v };
            // Gets the position where the key value pair is to be stored. 
            int position = Math.Abs(k.GetHashCode() % maxSize);
            // Checks if the key is already present
            // If key is repeated, old value is replaced with new value.
            for (int i = 0; i < list[position].Count; i++)
            {
                if (list[position][i].K.Equals(k))
                {
                    list[position][i].V = v;
                    duplicateFound = true;
                    break;
                }
            }
            // Adds the key value pair at calculated position.
            if (duplicateFound == false)
            {
                list[position].Add(current);
                counter++;
            }
            // Checks if rehashig is required.
            Rehashing();
        }

        /// <summary>
        /// checks if an entry with the given key exist?
        /// </summary>
        /// <param name="k">the key being sought</param>
        /// <returns>true iff the key exists in the table</returns>
        public bool Contains(Key k)
        {
            bool keyFound = false;
            // Calculates the position where to check for given key
            int position = Math.Abs(k.GetHashCode() % maxSize);
            // Checks at the calculated position if key is present
            foreach (KeyValuePair<Key, Value> key in list[position])
            {
                if (key.K.Equals(k))
                {
                    keyFound = true;
                    break;
                }
            }

            return keyFound;
            throw new NotImplementedException();
        }

        /// <summary>
        /// Fetch the value associated with the given key.
        /// </summary>
        /// <param name="k">The key to be looked up in the table</param>
        /// <returns>the value associated with the given key</returns>
        /// <exception cref="NonExistentKey">if Contains(key) is false</exception>
        public Value Get(Key k)
        {
            Value value;
            // Calculates the position where to check for given key
            int position = Math.Abs(k.GetHashCode() % maxSize);
            // If the key is present, returns the value associated to it
            foreach (KeyValuePair<Key, Value> key in list[position])
            {
                if (key.K.Equals(k))
                {
                    value = key.V;
                    return value;
                }
            }
            // Throws exception if key is not present.
            throw new NonExistentKey<Key>(k);
        }

        /// <summary>
        /// Iterates through entire table.
        /// </summary>
        /// <returns>The key of the key-value pair</returns>
        public IEnumerator<Key> GetEnumerator()
        {
            // Iterates through the entire list to return the key
            for (int i = 0; i < list.Length; i++)
            {
                for (int j = 0; j < list[i].Count; j++)
                {
                    yield return list[i][j].K;
                }  
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }

    class TableFactory
    {
        /// <summary>
        /// Create a Table.
        /// (The student is to put a line of code in this method corresponding to
        /// the name of the Table implementor s/he has designed.)
        /// </summary>
        /// <param name="K">the key type</param>
        /// <param name="V">the value type</param>
        /// <param name="capacity">The initial maximum size of the table</param>
        /// <param name="loadThreshold">
        /// The fraction of the table's capacity that when
        /// reached will cause a rebuild of the table to a 50% larger size
        /// </param>
        /// <returns>A new instance of Table</returns>
        public static Table<K, V> Make<K, V>(int capacity = 100, 
        double loadThreshold = 0.75)
        {
            return new LinkedHashedTable<K, V>(capacity, loadThreshold);
        }
    }

    class MainClass
    {
        public static void Main(string[] args)
        {
            Table<String, String> ht = TableFactory.Make<String, String>(4, 0.5);
            ht.Put("Joe", "Doe");
            ht.Put("Jane", "Brain");
            ht.Put("Chris", "Swiss");
            try
            {
                foreach (String first in ht)
                {
                    Console.WriteLine(first + " -> " + ht.Get(first));
                }
                Console.WriteLine("=========================");

                ht.Put("Wavy", "Gravy");
                ht.Put("Chris", "Bliss");
                foreach (String first in ht)
                {
                    Console.WriteLine(first + " -> " + ht.Get(first));
                }
                Console.WriteLine("=========================");

                Console.Write("Jane -> ");
                Console.WriteLine(ht.Get("Jane"));
                Console.Write("John -> ");
                Console.WriteLine(ht.Get("John"));
            }
            catch (NonExistentKey<String> nek)
            {
                Console.WriteLine(nek.Message);
                Console.WriteLine(nek.StackTrace);
            }

            Test_Table.test();
        }
    }

    /// <summary>
    /// This class implements 6 unit test cases to check the correctness of the program
    /// Credits for test case -> Lakhan Bhojwani
    /// </summary>
    class Test_Table
    {
        
        /// <summary>
        /// Creating new table where keys and values are integers.
        /// And running the test cases.
        /// </summary>
        public static void test()
        {
            int key = 0;
            Table<int, int> ht = TableFactory.Make<int, int>(5, 0.6);
            int putCounter = 0;
            // Adds 2000 entries to the table ht.
            for (int i = 0; i < 2000; i++)
            {
                ht.Put(i, i);
                putCounter++;
            }

            //Test case 1 checks if all the entries are added to the table
            if (putCounter == 2000)
                Console.WriteLine("Test Case 1- Using Put() to add 2000 values :: Passed");
            else
                Console.WriteLine("Test Case 1- Using Put() to add 2000 values :: Failed");
            try
            {
                int getCounter = 0;
                // Iterate through the table to get 20 values
                for (int i = 0; i < 2000; i = i + 100)
                {
                    if (ht.Get(i).Equals(i))
                    {
                        getCounter++;
                    }
                }

                // Test case 2 to check Get function
                if (getCounter == 20)
                {
                    Console.WriteLine("Test Case 2- Checking Get() to fetch 20 values :: Passed");
                }
                else
                {
                    Console.WriteLine("Test Case 2- checking Get() to fetch 20 values :: Failed");
                }
                
                key = 2010;
                // check for non-existing key
                if (ht.Get(key) == 2010)
                    Console.WriteLine("Test case  3 to get value at key " + key + " :: Failed ");
            }
            catch (NonExistentKey<int> nek)
            {
                // Test case 3 to test the exception for non-existing key
                if (key.Equals(nek.BadKey))
                    Console.WriteLine("Test case 3- to throw exception for non-existing key " 
                    + key + " :: Passed");
            }

            int containsCounter = 0;
            try
            {
                for (int i = 0; i < 50; i++)
                {
                    if (ht.Contains(i) == true)
                    {
                        containsCounter++;
                    }
                }

                // Test case 4 to check Contains function
                if (containsCounter == 50)
                    Console.WriteLine("Test case 4- to Check Contains() for 0 to 49 :: Passed");
                else
                    Console.WriteLine("Test case 4- to Check Contains() for 0 to 49 :: failed");

                // Test case 5 to check Contains function for non existing key
                if (ht.Contains(2010) == false)

                    Console.WriteLine("Test case 5- to check Contains() for non existing key :: Passed");
                else
                    Console.WriteLine("Test case 5- to check Contains() for non existing key :: Failed");

                int enumCounter = 0;

                // Iterate through all the elements in the table
                foreach (int first in ht)
                {
                    enumCounter++;
                }

                // Test case 6 to check Enumerate()
                if (enumCounter == 2000)
                {
                    Console.WriteLine("Test case 6- to check GetEnumerator() :: Passed");
                }
                else
                    Console.WriteLine("Test case 6- to check GetEnumerator() :: Failed");
                Console.ReadLine();

            }
            catch (NonExistentKey<int> nek)
            {
                Console.WriteLine(nek.Message);
                Console.WriteLine(nek.StackTrace);
            }
            Console.ReadLine();
        }
    }

}
