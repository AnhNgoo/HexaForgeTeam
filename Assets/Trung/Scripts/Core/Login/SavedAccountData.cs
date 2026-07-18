using System;
using System.Collections.Generic;

[Serializable]
public class SavedAccount
{
    public string username;
    public string password;
}

[Serializable]
public class SavedAccountList
{
    public List<SavedAccount> accounts =
        new List<SavedAccount>();
}