using System;
internal static class PetRosterTests
{
    static void Check(bool ok,string label) {if(!ok)throw new Exception(label);}
    static void Main()
    {
        PetRoster<object> pets=new PetRoster<object>();object a=new object(),b=new object(),c=new object();
        pets.Add(a);pets.Add(b);pets.Add(c);Check(pets.Count==3,"three independent pets");
        Check(pets.Remove(b)&&pets.Count==2,"remove selected only");
        Check(!pets.Remove(b)&&pets.Count==2,"duplicate close harmless");
        object[] snapshot=pets.Snapshot();pets.Remove(a);Check(snapshot.Length==2&&pets.Count==1,"stable shutdown snapshot");
        pets.Remove(c);Check(pets.Count==0,"last pet removed");
        pets.Add(a);bool duplicate=false;try {pets.Add(a);}catch(ArgumentException) {duplicate=true;}
        Check(duplicate&&pets.Count==1,"no duplicate registration");
        Console.WriteLine("PASS: add three, remove one preserves others, duplicate close, shutdown snapshot, last removal, duplicate registration");
    }
}
