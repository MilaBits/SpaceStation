using UnityEngine;

public abstract class Item : ScriptableObject {
    public int Id;
    public Sprite Image;
    public string Description;

    public string Examine() {
        return Description;
    }
}