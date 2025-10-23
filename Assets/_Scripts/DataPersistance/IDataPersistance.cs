namespace CarePackage.Persistance
{
    public interface IDataPersistance
    {
        void LoadData(GameData loadData);
        void SaveData(GameData saveData);
    }
}