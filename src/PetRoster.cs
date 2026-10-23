using System;
using System.Collections.Generic;
internal sealed class PetRoster<T> where T:class
{
    readonly List<T> pets=new List<T>();
    public int Count {get {return pets.Count;}}
    public void Add(T pet) {if(pet==null)throw new ArgumentNullException("pet");if(pets.Contains(pet))throw new ArgumentException("Already registered");pets.Add(pet);}
    public bool Remove(T pet) {return pets.Remove(pet);}
    public T[] Snapshot() {return pets.ToArray();}
}
