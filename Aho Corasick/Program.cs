var keywords = new List<string> { "he", "she", "his", "hers" };
var ac = new AhoCorasick(keywords);

var text = "ushers";
foreach (var index in ac.Search(text))
{
    Console.WriteLine($"Pattern {index} ({keywords[index]}) found in text");
}

/*
  Aho-Corasick Algoritması Nedir?
   
    Aho-Corasick algoritması, çoklu anahtar kelime arama problemini çözmek için kullanılan bir veri yapısı ve algoritmadır. 
    Genellikle büyük metinlerde veya veri akışlarında birden fazla anahtar kelimeyi hızlı bir şekilde aramak için kullanılır. 
    Bu algoritma, anahtar kelimeleri bir trie (ön ek ağacı) veri yapısına dönüştürür ve başarısızlık bağlantıları (failure links) ekleyerek aramayı verimli hale getirir.
   
    Algoritmanın Çalışma Adımları:
   
    1. **Trie Yapısının Oluşturulması**:
       - Anahtar kelimelerden oluşan bir liste alınır.
       - Her bir anahtar kelime için, karakter karakter ilerleyerek bir trie (ön ek ağacı) oluşturulur.
       - Her bir düğüm, çocukları ve bu düğümde sona eren anahtar kelimenin indeksini saklar.
   
    2. **Başarısızlık Bağlantılarının Oluşturulması**:
       - Kök düğümün çocukları için başarısızlık bağlantıları, kök düğüme bağlanacak şekilde ayarlanır.
       - Diğer düğümler için başarısızlık bağlantıları, belirli bir karakter için uygun bir üst düğüme (başarıyla eşleşen) bağlanır.
       - Bu bağlantılar, arama sırasında karakter eşleşmediğinde hızlıca geri dönmeyi sağlar.
   
    3. **Arama İşlemi**:
       - Metin üzerinde karakter karakter ilerlenir.
       - Her karakter için, mevcut düğümde karakterin çocukları arasında arama yapılır.
       - Eğer karakter mevcut değilse, başarısızlık bağlantısına geçilerek arama devam ettirilir.
       - Karakter bulunduğunda, bu düğümde saklanan tüm çıktılar (anahtar kelime indeksleri) toplanır ve sonuçlar döndürülür.
   
    Özetle, Aho-Corasick algoritması anahtar kelimeleri hızlı bir şekilde aramak için trie yapısını ve başarısızlık bağlantılarını kullanarak verimliliği artırır. 
    Bu algoritma, büyük metinlerde veya veri akışlarında çoklu anahtar kelimeleri etkili bir şekilde bulmak için idealdir.
 */

public class AhoCorasick
{
    // Node sınıfı, her bir düğüm için verileri ve bağlantıları tutar.
    private class Node
    {
        // Çocuk düğümleri tutan sözlük
        public Dictionary<char, Node> Children { get; } = new Dictionary<char, Node>();
        // Başarısızlık bağlantısı
        public Node FailureLink { get; set; }
        // Düğümdeki çıktılar, yani anahtar kelimelerin indeksleri
        public List<int> Output { get; } = new List<int>();
    }

    // Kök düğüm
    private readonly Node _root;

    public AhoCorasick(IEnumerable<string> keywords)
    {
        _root = new Node();
        // Trie veri yapısını oluştur
        BuildTrie(keywords);
        // Başarısızlık bağlantılarını oluştur
        BuildFailureLinks();
    }

    // Trie veri yapısını oluşturur
    private void BuildTrie(IEnumerable<string> keywords)
    {
        int index = 0;
        foreach (var keyword in keywords)
        {
            var current = _root;
            foreach (var ch in keyword)
            {
                // Karakter mevcut değilse yeni bir düğüm oluştur
                if (!current.Children.ContainsKey(ch))
                {
                    current.Children[ch] = new Node();
                }
                // Çocuğa geç
                current = current.Children[ch];
            }
            // Anahtar kelimenin bitiş noktasına indeks ekle
            current.Output.Add(index++);
        }
    }

    // Başarısızlık bağlantılarını oluşturur
    private void BuildFailureLinks()
    {
        var queue = new Queue<Node>();

        // Kök düğümün doğrudan çocuklarını kuyruğa ekle
        foreach (var child in _root.Children.Values)
        {
            child.FailureLink = _root;
            queue.Enqueue(child);
        }

        // Kuyruk boşalana kadar devam et
        while (queue.Count > 0)
        {
            var current = queue.Dequeue();

            foreach (var kvp in current.Children)
            {
                var child = kvp.Value;
                queue.Enqueue(child);

                // Başarısızlık bağlantısını bulmak için yukarı bak
                var failureLink = current.FailureLink;
                while (failureLink != null && !failureLink.Children.ContainsKey(kvp.Key))
                {
                    failureLink = failureLink.FailureLink;
                }

                // Başarısızlık bağlantısını ayarla
                child.FailureLink = failureLink?.Children.GetValueOrDefault(kvp.Key) ?? _root;
                // Çıkışları güncelle
                child.Output.AddRange(child.FailureLink.Output);
            }
        }
    }

    // Metinde anahtar kelimeleri arar
    public IEnumerable<int> Search(string text)
    {
        var current = _root;
        foreach (var ch in text)
        {
            // Karakter mevcut değilse başarısızlık bağlantısına geç
            while (current != null && !current.Children.ContainsKey(ch))
            {
                current = current.FailureLink;
            }

            // Eğer kök düğümdeysen, tekrar başla
            if (current == null)
            {
                current = _root;
                continue;
            }

            // Geçerli çocuk düğüm
            current = current.Children[ch];
            // Çıktıları döndür
            foreach (var patternIndex in current.Output)
            {
                yield return patternIndex;
            }
        }
    }
}