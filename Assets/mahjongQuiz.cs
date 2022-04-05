using UnityEngine;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using System.Security.Cryptography;
using System.Collections;

public class mahjongQuiz : MonoBehaviour
{   new public KMAudio audio;
    public KMBombInfo info;

    public int difficulty;
    private string difficultyName;

    static int easyIdCounter = 1;
    static int hardIdCounter = 1;
    static int scrambledIdCounter = 1;
    int moduleId;
    bool isSolved;
    private bool moduleSolved;
	public MeshRenderer[] meshRenderers;
    public MeshRenderer confirmMeshRenderer;
    public MeshRenderer clearMeshRenderer;
    public Material confirmTextMaterial;

	public Material tileMaterial;
    public Material confirmMaterial;
    public Material clearMaterial;
	private Material[] tileMaterials = new Material[47];
	public Texture[] tiles;
	public Texture[] tilesTinted;

	public KMSelectable[] tileButtons;

	private bool[] selectedTiles = new bool[34];

	public KMSelectable confirmButton;
	public KMSelectable clearButton;

    KMAudio.KMAudioRef audioRef;
    Hand moduleHand;
    Hand moduleHandCopy;

	List<Tile> solutionTiles;

    void Start()
    {
        Init();
    }

    void Init()
    {
        isSolved = false;
        if (difficulty == 1) {
            moduleId = easyIdCounter++;
            difficultyName = "Easy";
        } else if (difficulty == 2) {
            moduleId = hardIdCounter++;
            difficultyName = "Hard";
        } else if (difficulty == 3) {
            moduleId = scrambledIdCounter++;
            difficultyName = "Scrambled";
        } else {
            throw new System.Exception("Difficulty can only be 1/2/3");
        }

		// create hand and determine solution
        if (difficulty == 1) moduleHand = buildLevelOneHand();
        if (difficulty == 2) moduleHand = buildLevelTwoHand();
        if (difficulty == 3) moduleHand = buildLevelTwoHand();
		moduleHand.tiles.Sort();
        moduleHandCopy = new Hand(new List<Tile> (moduleHand.tiles)); // store a copy of the module so it can be printed after the scrambled hand
        CompleteSolution solution = getTenpaiTiles(moduleHand);
        solutionTiles = solution.getSolutionTiles();


        // scramble hand and log for difficulty 3
		if (difficulty == 3) {
            moduleHand.shuffle();
            Debug.LogFormat("[Mahjong Quiz {1} #{0}] Scrambled hand: {2}", moduleId, difficultyName, moduleHand.logHand());
        }

        // log hand, solution, and explanations
        Debug.LogFormat("[Mahjong Quiz {1} #{0}] Given hand: {2}", moduleId, difficultyName, moduleHandCopy.logHand());
        Debug.LogFormat("[Mahjong Quiz {1} #{0}] Solution: {2}", moduleId, difficultyName, solution.logSolutionTiles());
        foreach (var possibleSolution in solution.possibleSolutions) {
            possibleSolution.sortSolutionMelds();
            Debug.LogFormat("[Mahjong Quiz {1} #{0}] Explanation: {2}", moduleId, difficultyName, possibleSolution.logExplanation());
        }

		// set textures for input buttons
		for (int i = 0; i < 34; i++) {
            tileMaterials[i] = new Material(tileMaterial); // set tile's material from base
            tileMaterials[i].SetTexture("_MainTex", tiles[i]); // set tile material's texture (image)
            meshRenderers[i].material = tileMaterials[i]; // apply material to mesh renderer
        }
		// set textures for display hand
		for (int i = 34; i < 47; i++) {
            tileMaterials[i] = new Material(tileMaterial); // set tile's material from base
            tileMaterials[i].SetTexture("_MainTex", tiles[moduleHand.tiles[i - 34].textureId]); // set tile material's texture (image)
            meshRenderers[i].material = tileMaterials[i];
        }

        // set texture for confirm / clear button
        confirmMeshRenderer.material = confirmMaterial;
        clearMeshRenderer.material = confirmMaterial;

		// setup listeners for button presses
		for (int i = 0; i < tileButtons.Length; i++) {
			selectedTiles[i] = false;
			var buttonIndex = i;
            tileButtons[i].OnInteract += delegate () { PressInputTile(buttonIndex); return false; };
		}
        confirmButton.OnInteract += delegate () { PressSubmitButton(); return false; };
        clearButton.OnInteract += delegate () { PressClearButton(); return false; };

        // preselect correct tiles for debugging / help
		// preselectCorrectTiles();
    }

    void PressInputTile(int buttonId)
    {
        GetComponent<KMAudio>().PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
        GetComponent<KMSelectable>().AddInteractionPunch(0.2f);
        if (isSolved) return;
		if (selectedTiles[buttonId]) { // if tile was selected
			selectedTiles[buttonId] = false;
			meshRenderers[buttonId].material.SetTexture("_MainTex", tiles[buttonId]);
		} else { // if tile was unselected
			selectedTiles[buttonId] = true;
			meshRenderers[buttonId].material.SetTexture("_MainTex", tilesTinted[buttonId]);
		}
    }

	void PressSubmitButton() {
        GetComponent<KMAudio>().PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
        GetComponent<KMSelectable>().AddInteractionPunch(0.75f);
        if (isSolved) return;
		if (isSubmissionCorrect()) {
            GetComponent<KMBombModule>().HandlePass();
            isSolved = true;
            Debug.LogFormat("[Mahjong Quiz {1} #{0}] Solved! Answer submitted: {2}", moduleId, difficultyName, getSubmittedAnswer());
        }
		else {
            GetComponent<KMBombModule>().HandleStrike();
            Debug.LogFormat("[Mahjong Quiz {1} #{0}] Strike! Answer submitted: {2}", moduleId, difficultyName, getSubmittedAnswer());
        }
	}

    void PressClearButton() {
        GetComponent<KMAudio>().PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
        GetComponent<KMSelectable>().AddInteractionPunch(0.2f);
        if (isSolved) return;
		for (int i = 0; i < 34; i++) {
			if (selectedTiles[i]) {
                selectedTiles[i] = false;
                meshRenderers[i].material.SetTexture("_MainTex", tiles[i]);
            }
		}
    }

	bool isSubmissionCorrect() {
		for (int i = 0; i < 34; i++) {
			if (selectedTiles[i] != (bool) solutionTiles.Contains(allTiles.Find(t => t.textureId == i))) return false;
		}
		return true;
	}

    string getSubmittedAnswer() {
        string answer = "";
        for (int i = 0; i < 34; i++) {
            if (selectedTiles[i]) {
                if (answer != "") answer += " ";
                answer += allTiles[i].id;
            }
        }
        return answer;
    }


    // Random functions:
    public static Tile selectRandomTile(List<Tile> tiles) {
        var index = Random.Range(0, tiles.Count);
        return tiles[index];
    }

	public void preselectCorrectTiles() {
		foreach (var tile in solutionTiles) {
			selectedTiles[tile.textureId] = true;
			meshRenderers[tile.textureId].material.SetTexture("_MainTex", tilesTinted[tile.textureId]);			
		}
	}

	// gets the tiles that would make the given hand tenpai
	public CompleteSolution getTenpaiTiles(Hand hand) {
        var completeSolution = new CompleteSolution(hand);

		hand.tiles.Sort();

		var kokushiSolutions = getKokushiSolutions(hand);
		if (kokushiSolutions.Count > 0) {
            completeSolution.possibleSolutions = kokushiSolutions;
            return completeSolution;
        }

		PossibleSolution sevenPairsSolution = getSevenPairsSolution(hand);
        if (sevenPairsSolution != null) {
            completeSolution.possibleSolutions.Add(sevenPairsSolution);
        }

		foreach (var tile in allTiles) {
            if (sevenPairsSolution != null && sevenPairsSolution.tile.id == tile.id) continue; // there is already a 7pairs solution
			if (hand.tiles.Where(t => t.id == tile.id).Count() >= 4) continue; // if there are 4 of this tile it should not be a tenpai tile
			Hand testHand = new Hand(new List<Tile> (hand.tiles));
            PossibleSolution solution = new PossibleSolution(tile, hand, false, false, new List<List<Tile>>(), new List<List<Tile>>(), new List<Tile>(), false);
			testHand.addTile(tile);
            var result = isTenpai(solution, testHand);
			if (result.isValid) completeSolution.possibleSolutions.Add(result);
		}
		return completeSolution;
	}

	public List<PossibleSolution> getKokushiSolutions(Hand hand) {
        var possibleSolutions = new List<PossibleSolution>();
		var singlesCount = 0;
		var pairsCount = 0;
		foreach (var terminal in allTerminals) {
			if (hand.tiles.Where(tile => tile.id == terminal.id).Count() == 2) pairsCount++;
			if (hand.tiles.Where(tile => tile.id == terminal.id).Count() == 1) singlesCount++;
		}
		if (singlesCount == 13) {
            foreach (var tile in allTerminals) {
                PossibleSolution solution = new PossibleSolution(tile, hand, true, false, new List<List<Tile>>(), new List<List<Tile>>(), new List<Tile>(), true);
                possibleSolutions.Add(solution);
            }
            return possibleSolutions;
        }
		if (singlesCount == 11 && pairsCount == 1) {
            var kokushiTenpaiTile = allTerminals.Except(hand.tiles).ToList()[0];
            PossibleSolution solution = new PossibleSolution(kokushiTenpaiTile, hand, true, false, new List<List<Tile>>(), new List<List<Tile>>(), new List<Tile>(), true);
            possibleSolutions.Add(solution);
        }
		return possibleSolutions;
	}

	public PossibleSolution getSevenPairsSolution(Hand hand) {
		var singlesCount = 0;
		var single = (Tile) null;
        var pairs = new List<List<Tile>>();
		foreach (var tile in allTiles) {
            List<Tile> pairTiles = hand.tiles.Where(t => t.id == tile.id).ToList();
			if (hand.tiles.Where(t => t.id == tile.id).Count() == 2) pairs.Add(pairTiles);
			if (hand.tiles.Where(t => t.id == tile.id).Count() == 1) {
				singlesCount++;
				single = tile;
			}
		}
		if (singlesCount == 1 && pairs.Count == 6 && single != null) {
            pairs.Add(new List<Tile> {single, single});
            PossibleSolution solution = new PossibleSolution(single, hand, false, true, pairs, new List<List<Tile>>(), new List<Tile>(), true);
            return solution;
        }
		return null;
	}

	public PossibleSolution isTenpai(PossibleSolution solution, Hand hand, bool pairRemoved = false) {
		if (hand.tiles.Count == 0) {
            solution.isValid = true;
            return solution;
        }
        var solutionA = solution.createCopy();
		var handA = new Hand(new List<Tile>(hand.tiles));
        var pon = handA.removeAndReturnPon();
		if (pon.Count == 3) {
            solutionA.regularMelds.Add(pon);
            var result = isTenpai(solutionA, handA, pairRemoved);
			if (result.isValid) return result;
		}
        if (!pairRemoved) {
            var solutionB = solution.createCopy();
		    var handB = new Hand(new List<Tile>(hand.tiles));
            var pair = handB.removeAndReturnPair();
            if (pair.Count == 2) {
                solutionB.pair = pair;
                var result = isTenpai(solutionB, handB, true);
                if (result.isValid) return result;
            }
        }
        var solutionC = solution.createCopy();
		var handC = new Hand(new List<Tile>(hand.tiles));
        var chi = handC.removeAndReturnChi();
		if (chi.Count == 3) {
            solutionC.regularMelds.Add(chi);
            var result = isTenpai(solutionC, handC, pairRemoved);
			if (result.isValid) return result;
		}
		return solution;
	}

    // Tile Class:
    public class Tile : System.IComparable {
        public string id {get; set;}
        public int value {get;set;}
        public Suit suit {get;set;}
		public int textureId {get;set;}

        public Tile(string id, int value, Suit suit, int textureId) {
            this.id = id;
            this.value = value;
            this.suit = suit;
			this.textureId = textureId;
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

        public string handToString() {
            string result = "Hand: ";
            foreach (var tile in this.tiles) {
                result += tile.id;
            }
            return result;
        }

        public string logHand() {
            string result = "";
            foreach (var tile in this.tiles) {
                if (result != "") result += " ";
                result += tile.id;
            }
            return result;
        }

        // add a random meld to the hand. Each meld has an equal chance of being selected.
        public void addMeld(Tile givenTile = null, int proximity = 0, Suit? suit = null) {
            var looking = true;
            while (looking) {
                var possibleMelds = new List<List<Tile>>();
                var usableTiles = allTiles;
                if (proximity == 1 || proximity == 2 || proximity == 3) usableTiles = getUnrelatedTiles(proximity, suit);
				if (suit != null) usableTiles = usableTiles.Where(t => t.suit == suit).ToList();
                var randomTile = (givenTile == null) ? selectRandomTile(usableTiles) : givenTile;
                var numTiles = this.numberOfGivenTileInHand(randomTile);
                if (numTiles >= 4) continue;
                if (numTiles <= 1) {
                    possibleMelds.Add(new List<Tile>(){
                        new Tile(randomTile.id, randomTile.value, randomTile.suit, randomTile.textureId),
                        new Tile(randomTile.id, randomTile.value, randomTile.suit, randomTile.textureId),
                        new Tile(randomTile.id, randomTile.value, randomTile.suit, randomTile.textureId)
                    });
                }

                var chis = allChis.Where(c => c.Contains(randomTile) && !c.Except(usableTiles).Any()).ToList();
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

        // add a random iippeiko to the hand. Each meld has an equal chance of being selected.
        public void addIippeiko(Tile givenTile = null, int proximity = 0, Suit? suit = null) {
            var looking = true;
            while (looking) {
                var possibleMelds = new List<List<Tile>>();
                var usableTiles = allTiles;
                if (proximity == 1 || proximity == 2 || proximity == 3) usableTiles = getUnrelatedTiles(proximity, suit);
				if (suit != null) usableTiles = usableTiles.Where(t => t.suit == suit).ToList();
                var randomTile = (givenTile == null) ? selectRandomTile(usableTiles) : givenTile;
                var numTiles = this.numberOfGivenTileInHand(randomTile);
                if (numTiles >= 3) continue;

                var chis = allChis.Where(c => c.Contains(randomTile) && !c.Except(usableTiles).Any()).ToList();
                foreach (var chi in chis) {
                    var valid = true;
                    foreach (var tile in chi) {
                        if (this.numberOfGivenTileInHand(tile) >= 3) {
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
                    this.tiles.Add(tile);
                }
                looking = false;
            }
        }

        public void addPair(int proximity = 0, Suit? suit = null) {
            var looking = true;
            var usableTiles = allTiles;
			if (suit != null) usableTiles = allTiles.Where(t => t.suit == suit).ToList();
            if (proximity == 1 || proximity == 2 || proximity == 3) usableTiles = getUnrelatedTiles(proximity, suit);
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

        public List<Tile> getUnrelatedTiles(int proximity, Suit? suit = null) {
			List<Tile> tiles = allTiles;
			if (suit != null) tiles = tiles.Where(t => t.suit == suit).ToList();
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

        public List<Tile> removeAndReturnPon() {
            List<Tile> pon = new List<Tile>();
            if (this.tiles.Count < 3) return pon; // not enough tiles to remove pon
            for (int i = 0; i < this.tiles.Count; i++) {
                if (this.tiles.Where(t => t.id == this.tiles[i].id).Count() >= 3) {
                    pon.AddRange(this.tiles.GetRange(i, 3));
                    this.tiles.RemoveRange(i, 3);
                    return pon; // return populated pon
                }
            }
            return pon; // return empty pon, no pon was found
        }

        public List<Tile> removeAndReturnPair() {
            List<Tile> pair = new List<Tile>();
            if (this.tiles.Count < 2) return pair; // not enough tiles to remove pair
            for (int i = 0; i < this.tiles.Count; i++) {
                if (this.tiles.Where(t => t.id == this.tiles[i].id).Count() >= 2) {
                    pair.AddRange(this.tiles.GetRange(i, 2));
                    this.tiles.RemoveRange(i, 2);
                    return pair; // return populated pair
                }
            }
            return pair; // return empty pair, no pair was found
        }

        public List<Tile> removeAndReturnChi() {
            List<Tile> chi = new List<Tile>();
            if (this.tiles.Count < 3) return chi; // not enough tiles to remove pair
            for (int i = 0; i < this.tiles.Count; i++) {
                var tile = this.tiles[i];
                if (tile.suit == Suit.Honors || tile.value >= 8) continue; // honor tile or 8/9 value tile can't be start of a chi
                // find the chi that would need to be removed
                var chiToRemove = allChis.Find(c => c[0].id == tile.id);
                if (chiToRemove == null) continue; // no chi found (in case I missed something)
                var indexesToRemove = new List<int>();
                foreach (var chiTile in chiToRemove) {
                    var index = this.tiles.FindIndex(t => t.id == chiTile.id);
                    if (index < 0) break;
                    else indexesToRemove.Add(index);
                }
                if (indexesToRemove.Count == 3) { // all 3 chi tiles to remove were found
                    for (int j = 2; j >= 0; j--) {
                        chi.AddRange(this.tiles.GetRange(indexesToRemove[j], 1));
                        this.tiles.RemoveRange(indexesToRemove[j], 1);
                    }
                    return chi; // return populated chi
                }
            }
            return chi; // return empty chi, no chi was found
        }

        // shuffle the order of the tiles (code shamelessly copied from stackoverflow)
		public void shuffle() {
			RNGCryptoServiceProvider provider = new RNGCryptoServiceProvider();
			int n = tiles.Count;
			while (n > 1) {
				byte[] box = new byte[1];
				do provider.GetBytes(box);
				while (!(box[0] < n * (byte.MaxValue / n)));
				int k = (box[0] % n);
				n--;
				Tile value = tiles[k];
				tiles[k] = tiles[n];
				tiles[n] = value;
			}
		}

        //
        public void scrambleRandomTile() {
            if (tiles.Count < 1) return;
            tiles.Sort();
            var randomIndex = Random.Range(0, tiles.Count);
            var suitOfRandomTile = tiles[randomIndex].suit;
            tiles.RemoveAt(randomIndex);
            var looking = true;
            while (looking) {
                var randomValue = Random.Range(1,10);
                var replacementTile = allTiles.Find(t => t.value == randomValue && t.suit == suitOfRandomTile);
                if (tiles.FindAll(t => t.id == replacementTile.id).Count > 3) continue; // there are already more than 3 of this tile
                tiles.Add(replacementTile);
                looking = false;
            }
        }
    }

    public class CompleteSolution {

        public Hand hand {get;set;}
        public List<PossibleSolution> possibleSolutions {get;set;}

        public CompleteSolution(Hand hand) {
            this.hand = hand;
            this.possibleSolutions = new List<PossibleSolution>();
        }

        public List<Tile> getSolutionTiles() {
            List<Tile> tiles = new List<Tile>();
            foreach (var solution in this.possibleSolutions) {
                tiles.Add(solution.tile);
            }
            return tiles;
        }

        public string logSolutionTiles() {
            string result = "";
            foreach (var solution in this.possibleSolutions) {
                if (result != "") result += " ";
                result += solution.tile.id;
            }
            return result;
        }
    }

    public class PossibleSolution {
        public Tile tile {get;set;} // the tile that completes the hand below
        public Hand hand {get;set;}
        public bool isKokushi {get;set;}
        public bool isSevenPairs {get;set;}
        public List<List<Tile>> sevenPairs {get;set;}
        public List<List<Tile>> regularMelds {get;set;}
        public List<Tile> pair {get;set;}
        public bool isValid {get;set;}

        public PossibleSolution(Tile tile, Hand hand, bool isKokushi, bool isSevenPairs, List<List<Tile>> sevenPairs, List<List<Tile>> regularMelds, List<Tile> pair, bool isValid) {
            this.tile = tile;
            this.hand = hand;
            this.isKokushi = isKokushi;
            this.isSevenPairs = isSevenPairs;
            this.sevenPairs = sevenPairs;
            this.regularMelds = regularMelds;
            this.pair = pair;
            this.isValid = isValid;
        }

        // idk how to properly clone objects so I'm doing it the overkill way
        public PossibleSolution createCopy() {
            var newTile = this.tile;
            var newHand = new Hand(this.hand.tiles);
            var newIsKokushi = this.isKokushi;
            var newIsSevenPairs = this.isSevenPairs;
            var newSevenPairs = new List<List<Tile>>();
            foreach (var sevenPair in this.sevenPairs) {
                List<Tile> newSevenPair = new List<Tile>();
                foreach (var tile in pair) {
                    newSevenPair.Add(allTiles[allTiles.IndexOf(tile)]);
                }
                newSevenPairs.Add(newSevenPair);
            }
            var newRegularMelds = new List<List<Tile>>();
            foreach (var meld in this.regularMelds) {
                List<Tile> newMeld = new List<Tile>();
                foreach (var tile in meld) {
                    newMeld.Add(allTiles[allTiles.IndexOf(tile)]);
                }
                newRegularMelds.Add(newMeld);
            }
            var newPair = new List<Tile>();
            foreach (var tile in this.pair) {
                newPair.Add(allTiles[allTiles.IndexOf(tile)]);
            }
            var newIsValid = this.isValid;
            PossibleSolution copy = new PossibleSolution(newTile, newHand, newIsKokushi, newIsSevenPairs, newSevenPairs, newRegularMelds, newPair, newIsValid);
            return copy;
        }

        public void sortSolutionMelds() {
            if (isSevenPairs) {
                this.sevenPairs.Sort((a,b) => a[1].CompareTo(b[1]));
                foreach (var pair in this.sevenPairs) {
                    pair.Sort();
                }

            } else if (!isKokushi) {
                this.regularMelds.Sort((a,b) => a[1].CompareTo(b[1]));
                foreach (var meld in this.regularMelds) {
                    meld.Sort();
                }

            }
        }

        public string logExplanation() {
            var explanation = this.tile.id + " | ";
            var handCopy = new Hand(new List<Tile>(this.hand.tiles));
            if (isKokushi) {
                handCopy.addTile(this.tile);
                handCopy.tiles.Sort();
                foreach (var tile in handCopy.tiles) {
                    explanation += tile.id + " ";
                }
            } else if (isSevenPairs) {
                foreach (var pair in this.sevenPairs) {
                    explanation += pair[0].id + " ";
                    explanation += pair[1].id + " | ";
                }
                explanation = explanation.Substring(0, explanation.Length-2);
            } else {
                foreach (var meld in this.regularMelds) {
                    explanation += meld[0].id + " ";
                    explanation += meld[1].id + " ";
                    explanation += meld[2].id + " | ";
                }
                explanation += pair[0].id + " ";
                explanation += pair[1].id;
            }
            return explanation;
        }
    }

    public enum Suit {
        Characters,
        Balls,
        Sticks,
        Honors
    }

    public static Tile m1 = new Tile("1m", 1, Suit.Characters, 0);
    public static Tile m2 = new Tile("2m", 2, Suit.Characters, 1);
    public static Tile m3 = new Tile("3m", 3, Suit.Characters, 2);
    public static Tile m4 = new Tile("4m", 4, Suit.Characters, 3);
    public static Tile m5 = new Tile("5m", 5, Suit.Characters, 4);
    public static Tile m6 = new Tile("6m", 6, Suit.Characters, 5);
    public static Tile m7 = new Tile("7m", 7, Suit.Characters, 6);
    public static Tile m8 = new Tile("8m", 8, Suit.Characters, 7);
    public static Tile m9 = new Tile("9m", 9, Suit.Characters, 8);
    public static Tile p1 = new Tile("1p", 1, Suit.Balls, 9);
    public static Tile p2 = new Tile("2p", 2, Suit.Balls, 10);
    public static Tile p3 = new Tile("3p", 3, Suit.Balls, 11);
    public static Tile p4 = new Tile("4p", 4, Suit.Balls, 12);
    public static Tile p5 = new Tile("5p", 5, Suit.Balls, 13);
    public static Tile p6 = new Tile("6p", 6, Suit.Balls, 14);
    public static Tile p7 = new Tile("7p", 7, Suit.Balls, 15);
    public static Tile p8 = new Tile("8p", 8, Suit.Balls, 16);
    public static Tile p9 = new Tile("9p", 9, Suit.Balls, 17);
    public static Tile s1 = new Tile("1s", 1, Suit.Sticks, 18);
    public static Tile s2 = new Tile("2s", 2, Suit.Sticks, 19);
    public static Tile s3 = new Tile("3s", 3, Suit.Sticks, 20);
    public static Tile s4 = new Tile("4s", 4, Suit.Sticks, 21);
    public static Tile s5 = new Tile("5s", 5, Suit.Sticks, 22);
    public static Tile s6 = new Tile("6s", 6, Suit.Sticks, 23);
    public static Tile s7 = new Tile("7s", 7, Suit.Sticks, 24);
    public static Tile s8 = new Tile("8s", 8, Suit.Sticks, 25);
    public static Tile s9 = new Tile("9s", 9, Suit.Sticks, 26);
    public static Tile z1 = new Tile("1z", 1, Suit.Honors, 27);
    public static Tile z2 = new Tile("2z", 2, Suit.Honors, 28);
    public static Tile z3 = new Tile("3z", 3, Suit.Honors, 29);
    public static Tile z4 = new Tile("4z", 4, Suit.Honors, 30);
    public static Tile z5 = new Tile("5z", 5, Suit.Honors, 31);
    public static Tile z6 = new Tile("6z", 6, Suit.Honors, 32);
    public static Tile z7 = new Tile("7z", 7, Suit.Honors, 33);

    public static List<Tile> allTiles = new List<Tile> {
        m1, m2, m3, m4, m5, m6, m7, m8, m9,
        p1, p2, p3, p4, p5, p6, p7, p8, p9,
        s1, s2, s3, s4, s5, s6, s7, s8, s9,
        z1, z2, z3, z4, z5, z6, z7
    };
    public static List<Tile> allTerminals = new List<Tile> {
        m1, m9, p1, p9, s1, s9,
        z1, z2, z3, z4, z5, z6, z7
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

    public static Hand buildLevelOneHand() {
        int random = Random.Range(0, 100);
        if (random < 30) return buildEasyHand();
        else return buildMediumHand();
    }

    public Hand buildLevelTwoHand() {
        int random = Random.Range(0, 100);
        if (random < 20) return buildHardHand();
        else return buildExpertHand();
    }

    public Hand buildLevelThreeHand() {
        int random = Random.Range(0, 100);
        if (random < 10) return buildHardHand();
        else return buildExpertHand();
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

	public static Hand buildMediumHand() {
        int random = Random.Range(0, 100);
        if (random < 3) { // 22234 + 67 - 3
            var mediumHand = buildHandFromTemplate("2223467");
            mediumHand.addMeld(proximity: 2);
            mediumHand.addMeld(proximity: 2);
			return mediumHand;
		}
        if (random < 16) { // 22234 - 13
            var mediumHand = buildHandFromTemplate("22234");
            mediumHand.addPair(proximity: 2);
            mediumHand.addMeld(proximity: 2);
            mediumHand.addMeld(proximity: 2);
			return mediumHand;
		}
        if (random < 36) { // 23456 - 20
            var mediumHand = buildHandFromTemplate("23456");
            mediumHand.addPair(proximity: 2);
            mediumHand.addMeld(proximity: 2);
            mediumHand.addMeld(proximity: 2);
			return mediumHand;
		}
        if (random < 44) { // 23456789 - 8
            var mediumHand = buildHandFromTemplate("23456789");
            mediumHand.addPair(proximity: 2);
            mediumHand.addMeld(proximity: 2);
			return mediumHand;
		}
        if (random < 48) { // ambiguous 7 pairs? - 4
            var mediumHand = buildHandFromTemplate("22334");
            mediumHand.addIippeiko(proximity: 2);
            mediumHand.addPair(proximity: 2);
			return mediumHand;
		}
        if (random < 53) { // iipeiko 7 pairs - 5
            var mediumHand = buildHandFromTemplate("22334");
            mediumHand.addPair(proximity: 2);
            mediumHand.addPair(proximity: 2);
            mediumHand.addPair(proximity: 2);
            mediumHand.addPair(proximity: 2);
			return mediumHand;
		}
        if (random < 66) { // 2223 - 13
            var mediumHand = buildHandFromTemplate("2223");
            mediumHand.addMeld(proximity: 2);
            mediumHand.addMeld(proximity: 2);
            mediumHand.addMeld(proximity: 2);
			return mediumHand;
		}
        if (random < 79) { // 2224 - 13
            var mediumHand = buildHandFromTemplate("2224");
            mediumHand.addMeld(proximity: 3);
            mediumHand.addMeld(proximity: 3);
            mediumHand.addMeld(proximity: 3);
			return mediumHand;
		}
        if (random < 93) { // 2345 - 14
            var mediumHand = buildHandFromTemplate("2345");
            mediumHand.addMeld(proximity: 3);
            mediumHand.addMeld(proximity: 3);
            mediumHand.addMeld(proximity: 3);
			return mediumHand;
		}
        else { // 1 sided kokushi - 7
			var mediumHand = buildKokushiHand(1);
			return mediumHand;
		}
	}

    public Hand buildHardHand() {
        int random = Random.Range(0, 100);
        if (random < 7) { // 22234 + pair (25+pair)
            var hardHand = buildHandFromTemplate("22234");
            hardHand.addPair(proximity: 1);
            addHardMelds(hardHand);
			return hardHand;
		}
        if (random < 13) { // 2345567 (258)
            var hardHand = buildHandFromTemplate("2345567");
            addHardMelds(hardHand);
			return hardHand;
		}
        if (random < 19) { // 2333456 (147/2)
            var hardHand = buildHandFromTemplate("2333456");
            addHardMelds(hardHand);
			return hardHand;
		}
        if (random < 26) { // 2223456 (147/36)
            var hardHand = buildHandFromTemplate("2223456");
            addHardMelds(hardHand);
			return hardHand;
		}
        if (random < 33) { // 2224456 (3/47)
            var hardHand = buildHandFromTemplate("2224456");
            addHardMelds(hardHand);
			return hardHand;
		}
        if (random < 39) { // 2223344 (2345)
            var hardHand = buildHandFromTemplate("2223344");
            addHardMelds(hardHand);
			return hardHand;
		}
        if (random < 46) { // 2223444 (12345)
            var hardHand = buildHandFromTemplate("2223444");
            addHardMelds(hardHand);
			return hardHand;
		}
        if (random < 53) { // 2224666 (345)
            var hardHand = buildHandFromTemplate("2224666");
            addHardMelds(hardHand);
			return hardHand;
		}
        if (random < 59) { // 2233334 (14/25)
            var hardHand = buildHandFromTemplate("2233334");
            addHardMelds(hardHand);
			return hardHand;
		}
        if (random < 65) { // 2223345 (14/25)
            var hardHand = buildHandFromTemplate("2223345");
            addHardMelds(hardHand);
			return hardHand;
		}
        if (random < 71) { // 2223445 (36/4)
            var hardHand = buildHandFromTemplate("2223445");
            addHardMelds(hardHand);
			return hardHand;
		}
        if (random < 78) { // 2233344 (234)
            var hardHand = buildHandFromTemplate("2233344");
            addHardMelds(hardHand);
			return hardHand;
		}
        if (random < 84) { // 2223334 (2345)
            var hardHand = buildHandFromTemplate("2223334");
            addHardMelds(hardHand);
			return hardHand;
		}
        if (random < 90) { // 2234444 (25/3)
            var hardHand = buildHandFromTemplate("2234444");
            addHardMelds(hardHand);
			return hardHand;
		}
        if (random < 96) { // 2333345 (14/25)
            var hardHand = buildHandFromTemplate("2333345");
            addHardMelds(hardHand);
			return hardHand;
		}
        if (random < 99) { // 13 sided noten
            var hardHand = buildKokushiHand(13);
			return hardHand;
		}
        else { // noten kokushi
			var hardHand = buildKokushiHand(0);
			return hardHand;
		}
    }

    public void addHardMelds(Hand hand) {
        if (difficulty == 3) {
            Suit suit = hand.tiles[0].suit;
            hand.addMeld(proximity: 1, suit: suit);
        } else {
            hand.addMeld(proximity: 1);
        }
        hand.addMeld(proximity: 1);
    }

    // add two melds. If on difficulty level 3 the first meld should use the same suit as the template hand
    public Hand buildExpertHand() {
        int random = Random.Range(0, 100);
        if (random < 5) { // 9 sided wait, usually 9 gates
            return buildNineSideWait();
		}
        if (random < 45) { // random purity hand
            return buildPurityHand();
		}
        if (random < 52) {
            return buildNotenPurityHand();
        }
        if (difficulty == 3 && random < 55) {
            return buildNotenPurityHand();
        }
        else { // random template hand
            var randomIndex = Random.Range(0, expertTemplates.Count);
            var hardHand = buildHandFromTemplate(expertTemplates[randomIndex]);
            if (difficulty == 3 && random < 90) {
                Suit suit = hardHand.tiles[0].suit;
                hardHand.addMeld(proximity: 1, suit: suit);
            } else {
                hardHand.addMeld(proximity: 1);
            }
			return hardHand;
		}
    }

    public static List<string> expertTemplates = new List<string> {
        "2222333344", // (12345)
        "2222333345", // (1245)
        "2222333445", // (1346)
        "2222334455", // (25) // not interesting?
        "2222344455", // (145)
        "2223333445", // (23456)
        "2223333455", // (25)
        "2223334445", // (23456)
        "2223334455", // (13456)
        "2223334555", // (23456)
        "2223344445", // (123456)
        "2223344455", // (3456)
        "2223344555", // (2345)
        "2222333456", // (1247)
        "2222334456", // (13467)
        "2222345555", // (25)
        "2223334456", // (23457)
        "2223334566", // (36)
        "2223334556", // (457)
        "2223344456", // (23457)
        "2223344556", // (13467)
        "2223345556", // (35)
        "2223345566", // (47)
        "2223444456", // (1234567)
        "2223444556", // (3467)
        "2223444566", // (456)
        "2223445556", // (25)
        "2223445566", // (13467)
        "2223455566", // (256)
        "2223455666", // (2457)
        "2223334567", // (234578)
        "2223344567", // (2345)
        "2223345567", // (1346)
        "2223345677", // (37)
        "2223444567", // (123458)
        "2223445567", // (3467)
        "2223445677", // (257)
        "2223455567", // (25)
        "2223455677", // (467)
        "2223456667", // (25678)
        "2223456677", // (5678)
        "2223456777", // (12345678)
        "2223345678", // (13469)
        "2223445678", // (3469)
        "2223455678", // (258)
        "2223456678", // (134679)
        "2223456778", // (679)
        "2223456788", // (258)
        "2233445566", // (2356)
        "2233445567", // (258)
        "2233444567", // (2347)
        "2234445567", // (24)
        "2344455667", // (147)
        "2333456667", // (36)
        "2333455567", // (358)
        "2344455567", // (1458)
        "2344445567", // (23568)
        "2334444567", // (358)
        "2333345666", // (12457)
        "2223456789" // (134679)
    };
	
    // creates a random Hand based on a string template where all characters in the string are digits from 1-9
	public static Hand buildHandFromTemplate(string template) {
		var characters = template.ToArray().ToList();
		if (characters.Any(s => !char.IsDigit(s))) throw new System.Exception("not all characters in template are digits");
		List<int> numbers = new List<int>();
		foreach (var character in characters) {
			numbers.Add((int) char.GetNumericValue(character));
		}
		if (numbers.Any(n => n == 0)) throw new System.Exception("template string cannot contain 0's");
		if (numbers.Any(n => numbers.Where(x => x == n).Count() > 4)) throw new System.Exception("template cannot have more than 4 of the same tile");

		// shove all values to the left (ie. 44456 -> 11123) so they can be reliably and similarly manipulated
		int minimum = numbers.Min();
		if (minimum != 1) {
			for (int i = 0; i < numbers.Count; i++) {
				numbers[i] = numbers[i] - minimum + 1;
			}
		}

		// randomly shift the hand (25% chance to be on the edge, 75% to be central)
		// 50% chance to flip the tiles (ie. 1123 -> 7899)
		int move;
		int moveroom = 9 - numbers.Max();
		int edgeChance = Random.Range(0, 4);
		if (edgeChance == 0 || moveroom == 0) { // if the template spans 1-9, it should always be on the edge
			var leftEdgeChance = Random.Range(0,2);
			move = leftEdgeChance == 0 ? 0 : moveroom; // move amount is 0 or the maximum possible move room
		} else move = Random.Range(1, moveroom);
		var flipChance = Random.Range(0,2);
		for (int i = 0; i < numbers.Count; i++) {
			numbers[i] = numbers[i] + move;
			if (flipChance == 0) numbers[i] = 10 - numbers[i];
		}

		// pick random suit
		Suit suit = (Suit) Random.Range(0,3);

		// build a hand from the template after adjustments
		List<Tile> tiles = new List<Tile>();
		for (int i = 0; i < numbers.Count; i++) {
			Tile tile = allTiles.Find(t => t.value == numbers[i] && t.suit == suit);
			tiles.Add(tile);
		}
		return new Hand(tiles);
	}

	public static Hand buildPurityHand() {
        var purityHand = new Hand(new List<Tile>());
		Suit suit = (Suit) Random.Range(0,3);
		purityHand.addPair(suit: suit);
		purityHand.addMeld(suit: suit);
		purityHand.addMeld(suit: suit);
		purityHand.addMeld(suit: suit);
		purityHand.addMeld(suit: suit);
		int randomTileIndex = Random.Range(0, 14);
		purityHand.tiles.RemoveAt(randomTileIndex);
		return purityHand;
	}

    public Hand buildNineSideWait() {
        int random = Random.Range(0, 100);
        if (random < 40) { // 1112345678999 class 9 gates
            var hardHand = buildHandFromTemplate("1112345678999");
			return hardHand;
		}
        if (random < 52) { // 2223456777789
            var hardHand = buildHandFromTemplate("2223456777789");
			return hardHand;
		}
        if (random < 64) { // 1112345666678
            var hardHand = buildHandFromTemplate("1112345666678");
			return hardHand;
		}
        if (random < 76) { // 2223456677778
            var hardHand = buildHandFromTemplate("2223456677778");
			return hardHand;
		}
        if (random < 88) { // 2344445666678
            var hardHand = buildHandFromTemplate("2344445666678");
			return hardHand;
		}
        else { // 2333345677778
            var hardHand = buildHandFromTemplate("2333345677778");
			return hardHand;
		}
    }

    public Hand buildNotenPurityHand() {
        var purityHand = buildPurityHand();
        var looking = true;
        while (looking) { // keep scrambling one tile at a time until the hand is noten
            purityHand.scrambleRandomTile();
            if (getTenpaiTiles(purityHand).getSolutionTiles().Count == 0) return purityHand;
        }
        return purityHand; // this should theoretically never get reached
    }

	public static Hand buildTemplateHand() {
		var templateHand = buildHandFromTemplate("11123");
		templateHand.addPair(proximity: 2);
		templateHand.addMeld(proximity: 2);
		templateHand.addMeld(proximity: 2);
		return templateHand;
	}

	public static Hand buildKokushiHand(int waits) {
		if (waits == 0) {
			List<Tile> tiles = new List<Tile>(allTerminals);
			var index1ToReplace = Random.Range(0, allTerminals.Count);
            var index2ToReplace = Random.Range(0, allTerminals.Count);
            while (index2ToReplace == index1ToReplace) {
                index2ToReplace = Random.Range(0, allTerminals.Count);
            }
            var index1ToReplaceWith = Random.Range(0, allTerminals.Count);
            while (index1ToReplaceWith == index1ToReplace || index1ToReplaceWith == index2ToReplace ) {
                index2ToReplace = Random.Range(0, allTerminals.Count);
            }
            var index2ToReplaceWith = Random.Range(0, allTerminals.Count);
            while (index2ToReplaceWith == index1ToReplace || index2ToReplaceWith == index2ToReplace || index2ToReplaceWith == index1ToReplaceWith) {
                index2ToReplace = Random.Range(0, allTerminals.Count);
            }
			tiles[index1ToReplace] = allTerminals[index1ToReplaceWith];
			tiles[index2ToReplace] = allTerminals[index2ToReplaceWith];
			return new Hand(tiles);
		}
		if (waits == 1) {
			List<Tile> tiles = new List<Tile>(allTerminals);
			var indexToReplace = Random.Range(0, allTerminals.Count);
			var amountToSubtract = Random.Range(1, allTerminals.Count);
			var indexToUse = indexToReplace - amountToSubtract;
			if (indexToUse < 0) indexToUse += allTerminals.Count;
			tiles[indexToReplace] = allTerminals[indexToUse];
			return new Hand(tiles);
		}
		if (waits == 13) return new Hand(allTerminals);
		throw new System.Exception("cannot make kokushi hand that doesn't have 0/1/13 waits");
	}

    #pragma warning disable 414
    private string TwitchHelpMessage = "Toggle tiles with !{0} toggle 1p 5s 8s. Submit using !{0} submit. Clear selection using !{0} clear." + 
        "Character tiles are [1-9]m, circle tiles are [1-9]p, bamboo tiles are [1-9]s, and honor tiles (east/south/west/north/white/green/red) are [1-7]z" + 
        "You may also combine toggle and submit commands like so: !{0} submit 1m 3s 6s.";
    #pragma warning restore 414

    private IEnumerator ProcessTwitchCommand(string command) {
        command = command.ToLower().Trim().Replace(" ", "");
        if (Regex.IsMatch(command, @"^submit$")) {
            PressSubmitButton();
            yield return null;
        } else if (Regex.IsMatch(command, @"^clear$")) {
            PressClearButton();
            yield return null;
        } else {
            Regex tilesRegex = new Regex(@"(submit|toggle)((?:[1-9][mps]|[1-7]z)+)$");
            MatchCollection matches = tilesRegex.Matches(command);
            if (matches.Count == 1) {
                var groups = matches[0].Groups;
                if (groups.Count == 2 && groups[1] != null) {
                    var tiles = splitStringIntoPairs(groups[1].ToString());
                    foreach (var tileId in tiles) {
                        PressInputTile(allTiles.Find(x => x.id == tileId).textureId);
                        yield return null;
                    }
                    if (groups[0].ToString() == "submit") {
                        PressSubmitButton();
                        yield return null;
                    }
                }
            }            
        }
    }

    private List<string> splitStringIntoPairs(string tilesString) {
        var tiles = new List<string>();
        if (tilesString.Length % 2 != 0) throw new System.Exception("Tiles string: '" + tilesString + "' should have be an even amount. Likely a regex bug.");
        int tileCount = tilesString.Length / 2;
        for (int i = 0; i < tileCount; i++) {
            tiles.Add(tilesString.Substring(i, 2));
        }
        return tiles;
    }
}
