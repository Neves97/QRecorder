public class AudioDevice(int id, string name) // usando nova sintaxe de contrutores do C# 14.0 para simplificar a classe de modelo de dados
{
    public int Id { get; set; } = id;
    public string Name { get; set; } = name;
}