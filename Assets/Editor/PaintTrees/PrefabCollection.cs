using System.Collections.Generic;
using UnityEngine;
public class PrefabCollection
{
    public string name { get; set; }
    public List<string> collection;

    public PrefabCollection()
    {

        collection = new List<string>();
    }

}