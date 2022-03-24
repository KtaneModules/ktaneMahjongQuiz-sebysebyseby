using UnityEngine;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Linq;

public class ExampleModule2 : MonoBehaviour
{
    public KMSelectable[] buttons;
    KMAudio.KMAudioRef audioRef;
    int correctIndex;

    void Start()
    {
        Init();
    }

    void Init()
    {
        // test basic hands
        // var hand1 = new Hand(new List<Tile>{s1,s1,s1,s4,s4,s4,s7,s7,s7,m1,m1,m4,m4});
        // Debug.Log("Printing Hand 1: " + hand1.printHand());
        // var tenpaiTiles1 = getTenpaiTiles(hand1);
        // var tenpai1 = "";
        // foreach (var tile in tenpaiTiles1) tenpai1 += tile.id;
        // Debug.Log("Hand 1 tenpai tiles: " + tenpai1);

        // var hand2 = new Hand(new List<Tile>{s1,s1,s1,s2,s3,m1,m1});
        // Debug.Log("Printing Hand 2: " + hand2.printHand());
        // var tenpaiTiles2 = getTenpaiTiles(hand2);
        // var tenpai2 = "";
        // foreach (var tile in tenpaiTiles2) tenpai2 += tile.id;
        // Debug.Log("Hand 2 tenpai tiles: " + tenpai2);

        // var hand3 = new Hand(new List<Tile>{s1,s1,s1,s2,s3,s4,s5,s6,s7,s8,s9,s9,s9});
        // Debug.Log("Printing Hand 3: " + hand3.printHand());
        // var tenpaiTiles3 = getTenpaiTiles(hand3);
        // var tenpai3 = "";
        // foreach (var tile in tenpaiTiles3) tenpai3 += tile.id;
        // Debug.Log("Hand 3 tenpai tiles: " + tenpai3);

        // var hand4 = new Hand(new List<Tile>{s1,s1,s1,s2,s3,s7,s7,s7,m1,m1,m4,m4,m4});
        // Debug.Log("Printing Hand 4: " + hand4.printHand());
        // var tenpaiTiles4 = getTenpaiTiles(hand4);
        // var tenpai4 = "";
        // foreach (var tile in tenpaiTiles4) tenpai4 += tile.id;
        // Debug.Log("Hand 4 tenpai tiles: " + tenpai4);

        // // Kokushi checks
        // var hand5 = new Hand(new List<Tile>{s1,s9,p1,p9,m1,m9,east,south,west,north,green,red,white});
        // Debug.Log("Printing Hand 5: " + hand5.printHand());
        // var tenpaiTiles5 = getTenpaiTiles(hand5);
        // var tenpai5 = "";
        // foreach (var tile in tenpaiTiles5) tenpai5 += tile.id;
        // Debug.Log("Hand 5 tenpai tiles: " + tenpai5);

        // var hand6 = new Hand(new List<Tile>{s1,s9,p1,p9,m1,m9,east,south,west,green,green,red,white});
        // Debug.Log("Printing Hand 6: " + hand6.printHand());
        // var tenpaiTiles6 = getTenpaiTiles(hand6);
        // var tenpai6 = "";
        // foreach (var tile in tenpaiTiles6) tenpai6 += tile.id;
        // Debug.Log("Hand 6 tenpai tiles: " + tenpai6);

        // // 7 pairs checks
        // var hand7 = new Hand(new List<Tile>{s1,s1,s3,s3,s4,s4,s8,s8,s9,s9,p1,p1,p2});
        // Debug.Log("Printing Hand 7: " + hand7.printHand());
        // var tenpaiTiles7 = getTenpaiTiles(hand7);
        // var tenpai7 = "";
        // foreach (var tile in tenpaiTiles7) tenpai7 += tile.id;
        // Debug.Log("Hand 7 tenpai tiles: " + tenpai7);
        
        // var hand8 = new Hand(new List<Tile>{s1,s1,s3,s3,s4,s4,s8,s8,p1,p1,p1,p1,p2});
        // Debug.Log("Printing Hand 8: " + hand8.printHand());
        // var tenpaiTiles8 = getTenpaiTiles(hand8);
        // var tenpai8 = "";
        // foreach (var tile in tenpaiTiles8) tenpai8 += tile.id;
        // Debug.Log("Hand 8 tenpai tiles: " + tenpai8);

        // var hand9 = new Hand(new List<Tile>{s2,s2,s3,s3,s4,s4,s5,s5,s6,s6,s7,s7,s8});
        // Debug.Log("Printing Hand 9: " + hand9.printHand());
        // var tenpaiTiles9 = getTenpaiTiles(hand9);
        // var tenpai9 = "";
        // foreach (var tile in tenpaiTiles9) tenpai9 += tile.id;
        // Debug.Log("Hand 9 tenpai tiles: " + tenpai9);

        // print several random hands
        // for (int i = 0; i <30; i++) {
        //     var randomHand = buildEasyHand();
        //     Debug.Log("Printing random hand: " + randomHand.printHand());
        //     var waits = getTenpaiTiles(randomHand);
        //     var waitString = "";
        //     foreach (var tile in waits) {
        //         waitString += tile.id;
        //     }
        //     Debug.Log("Waits: " + waitString);
        // }
        
        var moduleHand = buildEasyHand();
        Debug.Log("Printing random hand: " + moduleHand.printHand());
        var solutionTiles = getTenpaiTiles(moduleHand);
        var waitString = "";
        foreach (var tile in solutionTiles) {
            waitString += tile.id;
        }
        Debug.Log("solution tiles: " + waitString);
        // modded example module:
        correctIndex = solutionTiles.Count;
        GetComponent<KMBombModule>().OnActivate += OnActivate;
        GetComponent<KMSelectable>().OnCancel += OnCancel;
        GetComponent<KMSelectable>().OnLeft += OnLeft;
        GetComponent<KMSelectable>().OnRight += OnRight;
        GetComponent<KMSelectable>().OnSelect += OnSelect;
        GetComponent<KMSelectable>().OnDeselect += OnDeselect;
        GetComponent<KMSelectable>().OnHighlight += OnHighlight;

        for (int i = 0; i < buttons.Length; i++)
        {
            string label = i.ToString();

            TextMesh buttonText = buttons[i].GetComponentInChildren<TextMesh>();
            buttonText.text = label;
            int j = i;
            buttons[i].OnInteract += delegate () { Debug.Log("Press #" + j); OnPress(j == correctIndex); return false; };
            buttons[i].OnInteractEnded += OnRelease;
        }


        // vanilla example module:
        // correctIndex = Random.Range(0, 4);
        // GetComponent<KMBombModule>().OnActivate += OnActivate;
        // GetComponent<KMSelectable>().OnCancel += OnCancel;
        // GetComponent<KMSelectable>().OnLeft += OnLeft;
        // GetComponent<KMSelectable>().OnRight += OnRight;
        // GetComponent<KMSelectable>().OnSelect += OnSelect;
        // GetComponent<KMSelectable>().OnDeselect += OnDeselect;
        // GetComponent<KMSelectable>().OnHighlight += OnHighlight;

        // for (int i = 0; i < buttons.Length; i++)
        // {
        //     string label = i == correctIndex ? "A" : "B";

        //     TextMesh buttonText = buttons[i].GetComponentInChildren<TextMesh>();
        //     buttonText.text = label;
        //     int j = i;
        //     buttons[i].OnInteract += delegate () { Debug.Log("Press #" + j); OnPress(j == correctIndex); return false; };
        //     buttons[i].OnInteractEnded += OnRelease;
        // }
    }

    private void OnDeselect()
    {
        Debug.Log("ExampleModule2 OnDeselect.");
    }

    private void OnLeft()
    {
        Debug.Log("ExampleModule2 OnLeft.");
    }

    private void OnRight()
    {
        Debug.Log("ExampleModule2 OnRight.");
    }

    private void OnSelect()
    {
        Debug.Log("ExampleModule2 OnSelect.");
    }

    private void OnHighlight()
    {
        Debug.Log("ExampleModule2 OnHighlight.");
    }

    void OnActivate()
    {
        foreach (string query in new List<string> { KMBombInfo.QUERYKEY_GET_BATTERIES, KMBombInfo.QUERYKEY_GET_INDICATOR, KMBombInfo.QUERYKEY_GET_PORTS, KMBombInfo.QUERYKEY_GET_SERIAL_NUMBER, "example"})
        {
            List<string> queryResponse = GetComponent<KMBombInfo>().QueryWidgets(query, null);

            if (queryResponse.Count > 0)
            {
                Debug.Log(queryResponse[0]);
            }
        }

        int batteryCount = 0;
        List<string> responses = GetComponent<KMBombInfo>().QueryWidgets(KMBombInfo.QUERYKEY_GET_BATTERIES, null);
        foreach (string response in responses)
        {
            Dictionary<string, int> responseDict = JsonConvert.DeserializeObject<Dictionary<string, int>>(response);
            batteryCount += responseDict["numbatteries"];
        }

        Debug.Log("Battery count: " + batteryCount);
    }

    bool OnCancel()
    {
        Debug.Log("ExampleModule2 cancel.");

        return true;
    }

    //On pressing button a looped sound will play
    void OnPress(bool correctButton)
    {
        Debug.Log("Pressed " + correctButton + " button");

        if (correctButton)
        {
            audioRef = GetComponent<KMAudio>().PlayGameSoundAtTransformWithRef(KMSoundOverride.SoundEffect.AlarmClockBeep, transform);
            GetComponent<KMBombModule>().HandlePass();
        }
        else
        {
            audioRef = GetComponent<KMAudio>().PlaySoundAtTransformWithRef("doublebeep125", transform);
        }
    }

    //On releasing a button a looped sound will stop
    void OnRelease()
    {
        Debug.Log("OnInteractEnded Released");
        if(audioRef != null && audioRef.StopSound != null)
        {
            audioRef.StopSound();
        }
    }


    // Random functions:

    
    public List<Tile> generateRandomListOfTiles() {
        var randomList = new List<Tile>();
        for (int i = 0; i < 20; i++) {
            randomList.Add(selectRandomTile(allTiles));
        }
        return randomList;
    }

    public static Tile selectRandomTile(List<Tile> tiles) {
        var index = Random.Range(0, tiles.Count);
        return tiles[index];
    }

    public static Hand buildEasyHand() {
        int random = Random.Range(0, 100);
        if (random < 18) { // shanpon wait
            var easyHand = new Hand(new List<Tile>());
            easyHand.addPair();
            easyHand.addPair(proximity: 1);
            easyHand.addMeld(proximity: 2);
            easyHand.addMeld(proximity: 2);
            easyHand.addMeld(proximity: 2);
            easyHand.tiles.Sort();
            return easyHand;
        }
        if (random < 30) { // eye wait
            var easyHand = new Hand(new List<Tile>());
            easyHand.tiles.Add(selectRandomTile(allTiles));
            easyHand.addMeld(proximity: 3);
            easyHand.addMeld(proximity: 3);
            easyHand.addMeld(proximity: 3);
            easyHand.addMeld(proximity: 3);
            easyHand.tiles.Sort();
            return easyHand;
        }
        if (random < 54) { // 2 sided sequence
            var easyHand = new Hand(new List<Tile>());
            easyHand.addTwoSidedWait();
            easyHand.addPair(proximity: 2);
            easyHand.addMeld(proximity: 2);
            easyHand.addMeld(proximity: 2);
            easyHand.addMeld(proximity: 2);
            return easyHand;
        }
        if (random < 60) { // 1 sided unambiguous 7 pairs
            var easyHand = new Hand(new List<Tile>());
            easyHand.tiles.Add(selectRandomTile(allTiles));
            easyHand.addPair(proximity: 1);
            easyHand.addPair(proximity: 2);
            easyHand.addPair(proximity: 2);
            easyHand.addPair(proximity: 2);
            easyHand.addPair(proximity: 2);
            easyHand.addPair(proximity: 2);
            return easyHand;
        }
        if (random < 84) { // 1 sided middle wait
            var easyHand = new Hand(new List<Tile>());
            easyHand.addMiddleWait();
            easyHand.addPair(proximity: 1);
            easyHand.addMeld(proximity: 2);
            easyHand.addMeld(proximity: 2);
            easyHand.addMeld(proximity: 2);
            return easyHand;
        }
        else { // 1 sided edge wait
            var easyHand = new Hand(new List<Tile>());
            easyHand.addEdgeWait();
            easyHand.addPair(proximity: 2);
            easyHand.addMeld(proximity: 2);
            easyHand.addMeld(proximity: 2);
            easyHand.addMeld(proximity: 2);
            return easyHand;
        }
    }

        public List<Tile> getTenpaiTiles(Hand hand) {
            hand.tiles.Sort();
            var tenpaiTiles = new List<Tile>();
            if (hand.tiles.Count != 13) return tenpaiTiles;

            var kokushiTiles = getKokushiTenpaiTiles(hand);
            if (kokushiTiles.Count > 0) return kokushiTiles;

            var sevenPairsTiles = getSevenPairsTiles(hand);

            foreach (var tile in allTiles) {
                // Debug.Log("testing tile: " + tile.id + " with hand: " + hand.printHand());
                if (hand.tiles.Where(t => t.id == tile.id).Count() >= 4) continue; // if there are 4 of this tile it should not be a tenpai tile
                Hand testHand = new Hand(new List<Tile> (hand.tiles));
                testHand.addTile(tile);
                if (isTenpai(testHand)) tenpaiTiles.Add(tile);
            }
            return tenpaiTiles.Union(sevenPairsTiles).ToList();
        }

        public List<Tile> getKokushiTenpaiTiles(Hand hand) {
            var singlesCount = 0;
            var pairsCount = 0;
            foreach (var terminal in allTerminals) {
                if (hand.tiles.Where(tile => tile.id == terminal.id).Count() == 2) pairsCount++;
                if (hand.tiles.Where(tile => tile.id == terminal.id).Count() == 1) singlesCount++;
            }
            if (singlesCount == 13) return allTerminals;
            if (singlesCount == 11 && pairsCount == 1) return allTerminals.Except(hand.tiles).ToList();
            return new List<Tile>();
        }

        public List<Tile> getSevenPairsTiles(Hand hand) {
            var singlesCount = 0;
            var single = (Tile) null;
            var pairsCount = 0;
            foreach (var tile in allTiles) {
                if (hand.tiles.Where(t => t.id == tile.id).Count() == 2) pairsCount++;
                if (hand.tiles.Where(t => t.id == tile.id).Count() == 1) {
                    singlesCount++;
                    single = tile;
                }
            }
            if (singlesCount == 1 && pairsCount == 6 && single != null) return new List<Tile>{single};
            return new List<Tile>();
        }

        public bool isTenpai(Hand hand, bool pairRemoved = false) {
            if (hand.tiles.Count == 0) return true;
            // Debug.Log("given hand: " + hand.printHand());
            var handA = new Hand(new List<Tile>(hand.tiles));
            // Debug.Log("attempting to remove pon");
            if (handA.removePon()) {
                // Debug.Log("hand A: " + handA.printHand());
                if (isTenpai(handA, pairRemoved)) return true;
            }
            var handB = new Hand(new List<Tile>(hand.tiles));
            // Debug.Log("attempting to remove pair");
            if (!pairRemoved && handB.removePair()) {
                // Debug.Log("hand B: " + handB.printHand());
                if (isTenpai(handB, true)) return true;
            }
            var handC = new Hand(new List<Tile>(hand.tiles));
            // Debug.Log("attempting to remove chi");
            if (handC.removeChi()) {
                // Debug.Log("hand C: " + handC.printHand());
                if (isTenpai(handC, pairRemoved)) return true;
            }
            // Debug.Log("returning false");
            return false;
        }

    // Tile Class:

    public class Tile : System.IComparable {
        public string id {get; set;}
        public int value {get;set;}
        public Suit suit {get;set;}

        public Tile(string id, int value, Suit suit) {
            this.id = id;
            this.value = value;
            this.suit = suit;
        }

        public override string ToString()
        {
            return this.id;
        }

        public Tile getTileBelow() {
            if (this.suit == Suit.Honors) return null;
            if (this.value == 1) return null;
            else return allTiles.Find(t => t.suit == this.suit && t.value - 1 == this.value);
        }

        public Tile getTileAbove() {
            if (this.suit == Suit.Honors) return null;
            if (this.value == 9) return null;
            else return allTiles.Find(t => t.suit == this.suit && t.value + 1 == this.value);
        }

        public override bool Equals(object obj)
        {
            Tile tile = obj as Tile;
            return tile != null &&
                   id == tile.id &&
                   value == tile.value &&
                   suit == tile.suit;
        }

        public override int GetHashCode()
        {
            int hashCode = 430968762;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(id);
            hashCode = hashCode * -1521134295 + value.GetHashCode();
            hashCode = hashCode * -1521134295 + suit.GetHashCode();
            return hashCode;
        }

        public int CompareTo(object obj)
        {
            Tile compareTile = obj as Tile;
            if (compareTile == null) return 1;
            if ((int) this.suit > (int) compareTile.suit) return 1;
            if ((int) this.suit < (int) compareTile.suit) return -1;
            if (this.value == compareTile.value) return 0;
            if (this.value > compareTile.value) return 1;
            else return -1;
        }
    }

    // Hand Class

    public class Hand {
        public List<Tile> tiles {get;set;}

        public Hand(List<Tile> tiles) {
            this.tiles = tiles;
        }

        public int numberOfGivenTileInHand(Tile tile) {
            return this.tiles.Where(t => t.id == tile.id).Count();
        }

        public void addTile(Tile tile) {
            this.tiles.Add(tile);
            this.tiles.Sort();
        }

        public string printHand() {
            var result = "Hand: ";
            foreach (var tile in tiles) {
                result += tile.id;
            }
            return result;
        }

        // add a random meld to the hand. Each meld has an equal chance of being selected.
        public void addMeld(Tile givenTile = null, int proximity = 0) {
            var looking = true;
            while (looking) {
                var possibleMelds = new List<List<Tile>>();
                var usableTiles = allTiles;
                if (proximity == 1 || proximity == 2 || proximity == 3) usableTiles = getUnrelatedTiles(proximity);
                var randomTile = (givenTile == null) ? selectRandomTile(usableTiles) : givenTile;
                var numTiles = this.numberOfGivenTileInHand(randomTile);
                if (numTiles >= 4) continue;
                if (numTiles <= 1) {
                    possibleMelds.Add(new List<Tile>(){
                        new Tile(randomTile.id, randomTile.value, randomTile.suit),
                        new Tile(randomTile.id, randomTile.value, randomTile.suit),
                        new Tile(randomTile.id, randomTile.value, randomTile.suit)
                    });
                }

                var chis = allChis.Where(c => c.Contains(randomTile)).ToList();
                foreach (var chi in chis) {
                    var valid = true;
                    foreach (var tile in chi) {
                        if (this.numberOfGivenTileInHand(tile) >= 4) {
                            valid = false;
                            break;
                        };
                    }
                    if (valid) possibleMelds.Add(chi);
                }
                var index = Random.Range(0, possibleMelds.Count);
                if (possibleMelds.Count <= 0) continue;
                var selected = possibleMelds[index];
                foreach (var tile in selected) {
                    this.tiles.Add(tile);
                }
                looking = false;
            }
        }

        public void addPair(int proximity = 0) {
            var looking = true;
            var usableTiles = allTiles;
            if (proximity == 1 || proximity == 2 || proximity == 3) usableTiles = getUnrelatedTiles(proximity);
            while (looking) {
                var randomTile = selectRandomTile(usableTiles);
                var numTiles = this.numberOfGivenTileInHand(randomTile);
                if (numTiles >= 3) continue;
                tiles.Add(randomTile);
                tiles.Add(randomTile);
                looking = false;
            }

        }

        public void addEdgeWait() {
            var random = Random.Range(0, 6);
            if (random == 0) {
                this.tiles.Add(m1);
                this.tiles.Add(m2);
            }
            if (random == 1) {
                this.tiles.Add(m8);
                this.tiles.Add(m9);
            }
            if (random == 2) {
                this.tiles.Add(p1);
                this.tiles.Add(p2);
            }
            if (random == 3) {
                this.tiles.Add(p8);
                this.tiles.Add(p9);
            }
            if (random == 4) {
                this.tiles.Add(s1);
                this.tiles.Add(s2);
            }
            if (random == 5) {
                this.tiles.Add(s8);
                this.tiles.Add(s9);
            }
        }

        public void addMiddleWait() {
            var index = Random.Range(0, allChis.Count);
            var randomChi = allChis[index];
            this.tiles.Add(randomChi[0]);
            this.tiles.Add(randomChi[2]);
        }

        public void addTwoSidedWait() {
            var index = Random.Range(0, allChis.Count);
            var randomChi = allChis[index];
            var highOrLow = Random.Range(0, 2);
            if (highOrLow == 0) {
                this.tiles.Add(randomChi[0]);
                this.tiles.Add(randomChi[1]);
            } else {
                this.tiles.Add(randomChi[1]);
                this.tiles.Add(randomChi[2]);
            }
        }

        public List<Tile> getUnrelatedTiles(int proximity) {
            if (proximity == 1) {
                return allTiles.Except(this.tiles).ToList();
            }
            if (proximity == 2) {
                var relatedTiles = new List<Tile>();
                foreach (var tile in this.tiles) {
                    relatedTiles.Add(tile);
                    var downTile = tile.getTileBelow();
                    if (downTile != null) relatedTiles.Add(downTile);
                    var upTile = tile.getTileAbove();
                    if (upTile != null) relatedTiles.Add(upTile);
                }
                return allTiles.Except(relatedTiles).ToList();
            }
            if (proximity == 3) {
                var relatedTiles = new List<Tile>();
                foreach (var tile in this.tiles) {
                    relatedTiles.Add(tile);
                    var downTile = tile.getTileBelow();
                    if (downTile != null) {
                        relatedTiles.Add(downTile);
                        var moreDownTile = downTile.getTileBelow();
                        if (moreDownTile != null) relatedTiles.Add(moreDownTile);
                    }
                    var upTile = tile.getTileBelow();
                    if (upTile != null) {
                        relatedTiles.Add(upTile);
                        var moreUpTile = upTile.getTileBelow();
                        if (moreUpTile != null) relatedTiles.Add(moreUpTile);
                    }
                }
                return allTiles.Except(relatedTiles).ToList();
            }
            else throw new System.Exception("Should not use proximity > 4");
        }

        public bool removePon() {
            if (this.tiles.Count < 3) return false; // not enough tiles to remove pon
            for (int i = 0; i < this.tiles.Count; i++) {
                if (this.tiles.Where(t => t.id == this.tiles[i].id).Count() >= 3) {
                    this.tiles.RemoveRange(i, 3);
                    return true;
                }
            }
            return false;
        }

        public bool removePair() {
            if (this.tiles.Count < 2) return false; // not enough tiles to remove pair
            for (int i = 0; i < this.tiles.Count; i++) {
                if (this.tiles.Where(t => t.id == this.tiles[i].id).Count() >= 2) {
                    this.tiles.RemoveRange(i, 2);
                    return true;
                }
            }
            return false;
        }

        public bool removeChi() {
            if (this.tiles.Count < 3) return false; // not enough tiles to remove pair
            for (int i = 0; i < this.tiles.Count; i++) {
                var tile = this.tiles[i];
                if (tile.suit == Suit.Honors || tile.value >= 8) continue; // honor tile or 8/9 value tile can't be start of a chi
                // find the chi that would need to be removed
                var chiToRemove = allChis.Find(chi => chi[0].id == tile.id);
                if (chiToRemove == null) continue; // no chi found (in case I missed something)
                var indexesToRemove = new List<int>();
                foreach (var chiTile in chiToRemove) {
                    var index = this.tiles.FindIndex(t => t.id == chiTile.id);
                    if (index < 0) break;
                    else indexesToRemove.Add(index);
                }
                if (indexesToRemove.Count == 3) { // all 3 chi tiles to remove were found
                    for (int j = 2; j >= 0; j--) {
                        this.tiles.RemoveRange(indexesToRemove[j], 1);
                    }
                    return true;
                }
            }
            return false;
        }
    }

    public enum Suit {
        Characters,
        Balls,
        Sticks,
        Honors
    }

    public static Tile m1 = new Tile("1m", 1, Suit.Characters);
    public static Tile m2 = new Tile("2m", 2, Suit.Characters);
    public static Tile m3 = new Tile("3m", 3, Suit.Characters);
    public static Tile m4 = new Tile("4m", 4, Suit.Characters);
    public static Tile m5 = new Tile("5m", 5, Suit.Characters);
    public static Tile m6 = new Tile("6m", 6, Suit.Characters);
    public static Tile m7 = new Tile("7m", 7, Suit.Characters);
    public static Tile m8 = new Tile("8m", 8, Suit.Characters);
    public static Tile m9 = new Tile("9m", 9, Suit.Characters);
    public static Tile p1 = new Tile("1p", 1, Suit.Balls);
    public static Tile p2 = new Tile("2p", 2, Suit.Balls);
    public static Tile p3 = new Tile("3p", 3, Suit.Balls);
    public static Tile p4 = new Tile("4p", 4, Suit.Balls);
    public static Tile p5 = new Tile("5p", 5, Suit.Balls);
    public static Tile p6 = new Tile("6p", 6, Suit.Balls);
    public static Tile p7 = new Tile("7p", 7, Suit.Balls);
    public static Tile p8 = new Tile("8p", 8, Suit.Balls);
    public static Tile p9 = new Tile("9p", 9, Suit.Balls);
    public static Tile s1 = new Tile("1s", 1, Suit.Sticks);
    public static Tile s2 = new Tile("2s", 2, Suit.Sticks);
    public static Tile s3 = new Tile("3s", 3, Suit.Sticks);
    public static Tile s4 = new Tile("4s", 4, Suit.Sticks);
    public static Tile s5 = new Tile("5s", 5, Suit.Sticks);
    public static Tile s6 = new Tile("6s", 6, Suit.Sticks);
    public static Tile s7 = new Tile("7s", 7, Suit.Sticks);
    public static Tile s8 = new Tile("8s", 8, Suit.Sticks);
    public static Tile s9 = new Tile("9s", 9, Suit.Sticks);
    public static Tile east = new Tile("e", 1, Suit.Honors);
    public static Tile south = new Tile("s", 2, Suit.Honors);
    public static Tile west = new Tile("w", 3, Suit.Honors);
    public static Tile north = new Tile("n", 4, Suit.Honors);
    public static Tile white = new Tile("wh", 5, Suit.Honors);
    public static Tile green = new Tile("g", 6, Suit.Honors);
    public static Tile red = new Tile("r", 7, Suit.Honors);

    public static List<Tile> allTiles = new List<Tile> {
        m1, m2, m3, m4, m5, m6, m7, m8, m9,
        p1, p2, p3, p4, p5, p6, p7, p8, p9,
        s1, s2, s3, s4, s5, s6, s7, s8, s9,
        east, south, west, north, white, green, red
    };
    public static List<Tile> allTerminals = new List<Tile> {
        m1, m9, p1, p9, s1, s9,
        east, south, west, north, white, green, red
    };

    public static List<List<Tile>> allChis = new List<List<Tile>> {
        new List<Tile> { m1, m2, m3 },
        new List<Tile> { m2, m3, m4 },
        new List<Tile> { m3, m4, m5 },
        new List<Tile> { m4, m5, m6 },
        new List<Tile> { m5, m6, m7 },
        new List<Tile> { m6, m7, m8 },
        new List<Tile> { m7, m8, m9 },
        new List<Tile> { p1, p2, p3 },
        new List<Tile> { p2, p3, p4 },
        new List<Tile> { p3, p4, p5 },
        new List<Tile> { p4, p5, p6 },
        new List<Tile> { p5, p6, p7 },
        new List<Tile> { p6, p7, p8 },
        new List<Tile> { p7, p8, p9 },
        new List<Tile> { s1, s2, s3 },
        new List<Tile> { s2, s3, s4 },
        new List<Tile> { s3, s4, s5 },
        new List<Tile> { s4, s5, s6 },
        new List<Tile> { s5, s6, s7 },
        new List<Tile> { s6, s7, s8 },
        new List<Tile> { s7, s8, s9 }
    };
}
