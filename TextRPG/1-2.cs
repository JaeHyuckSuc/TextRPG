using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using TextRPG;

namespace TextRPG
{
    internal class _1_2
    {
        static void Main(string[] args)
        {
            Game game = new Game();
            game.Start();
        }
    }

    public class Game
    {
        Character player;
        Shop shop;

        public Game()
        {
            player = new Character("Evangeline", "전사");
            shop = new Shop(player);
        }

        public void Start()
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("스파르타 마을에 오신 여러분 환영합니다.");
                Console.WriteLine("이곳에서 던전으로 들어가기전 활동을 할 수 있습니다.\n");
                Console.WriteLine("1. 상태창");
                Console.WriteLine("2. 인벤토리");
                Console.WriteLine("3. 상점");
                Console.WriteLine("4. 던전입장");
                Console.WriteLine("5. 휴식하기");
                Console.WriteLine("\n원하시는 행동을 입력해주세요.");
                Console.Write(">> ");

                string input = Console.ReadLine();
                switch (input)
                {
                    case "1": player.OpenStatus(); break;
                    case "2": player.Inventory(); break;
                    case "3": shop.OpenShop(); break;
                    case "4": EnterDungeon(); break;
                    case "5": Rest(); break;
                    default:
                        Console.WriteLine("잘못된 입력입니다.");
                        Console.ReadKey();
                        break;
                }
            }
        }

        private void Rest()
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("휴식하기");
                Console.WriteLine($"500 G 를 내면 체력을 회복할 수 있습니다. (보유 골드 : {player.Gold} G)");
                Console.WriteLine("\n1. 휴식하기");
                Console.WriteLine("0. 나가기");
                Console.Write("\n원하시는 행동을 입력해주세요.\n>> ");

                string input = Console.ReadLine();

                if (input == "0") break;
                else if (input == "1")
                {
                    if (player.Gold >= 500)
                    {
                        player.Gold -= 500;
                        player.RestoreHp();
                        Console.WriteLine("\n휴식을 완료했습니다.");
                    }
                    else
                    {
                        Console.WriteLine("\nGold 가 부족합니다.");
                    }
                    Console.ReadKey();
                }
                else
                {
                    Console.WriteLine("잘못된 입력입니다.");
                    Console.ReadKey();
                }
            }
        }

        private void EnterDungeon()
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("던전입장");
                Console.WriteLine("이곳에서 던전으로 들어가기전 활동을 할 수 있습니다.\n");
                Console.WriteLine("1. 쉬운 던전\t | 방어력 5 이상 권장");
                Console.WriteLine("2. 일반 던전\t | 방어력 11 이상 권장");
                Console.WriteLine("3. 어려운 던전\t | 방어력 17 이상 권장");
                Console.WriteLine("0. 나가기\n");
                Console.Write("원하시는 행동을 입력해주세요.\n>> ");

                string input = Console.ReadLine();
                if (input == "0") break;

                int requiredDef = 0;
                int baseReward = 0;
                string dungeonName = "";

                switch (input)
                {
                    case "1": requiredDef = 5; baseReward = 1000; dungeonName = "쉬운 던전"; break;
                    case "2": requiredDef = 11; baseReward = 1700; dungeonName = "일반 던전"; break;
                    case "3": requiredDef = 17; baseReward = 2500; dungeonName = "어려운 던전"; break;
                    default:
                        Console.WriteLine("잘못된 입력입니다.");
                        Console.ReadKey();
                        continue;
                }

                DoDungeon(requiredDef, baseReward, dungeonName);
            }
        }

        private void DoDungeon(int recommendedDef, int baseReward, string dungeonName)
        {
            Random rand = new Random();
            int totalDef = player.BaseDef + player.GetBonusDef();
            int totalAtk = player.BaseAtk + player.GetBonusAtk();
            int beforeHp = player.Hp;
            int beforeGold = player.Gold;

            bool isFail = totalDef < recommendedDef && rand.Next(100) < 40;

            int hpLossBase = rand.Next(20, 36);
            int defDiff = recommendedDef - totalDef;
            hpLossBase += defDiff;

            if (hpLossBase < 0) hpLossBase = 0;
            int actualHpLoss = isFail ? hpLossBase / 2 : hpLossBase;
            player.ReduceHp(actualHpLoss);

            if (isFail)
            {
                Console.WriteLine("\n던전 실패");
                Console.WriteLine("방어력이 부족하여 던전 공략에 실패했습니다...");
                Console.WriteLine($"체력 {beforeHp} -> {player.Hp}");
            }
            else
            {
                int minPercent = 0;
                int maxPercent = 0;

                switch (baseReward)
                {
                    case 1000: minPercent = 50; maxPercent = 150; break;   // 쉬운 던전: 50~150%
                    case 1700: minPercent = 75; maxPercent = 200; break;   // 일반 던전: 75~200%
                    case 2500: minPercent = 100; maxPercent = 250; break;  // 어려운 던전: 100~250%
                }

                int bonusPercent = rand.Next(minPercent, maxPercent + 1);
                int bonusGold = baseReward * bonusPercent / 100;
                int totalGold = baseReward + bonusGold;

                player.Gold += totalGold;
                player.AddDungeonClear();

                Console.Clear();
                Console.WriteLine("던전 클리어");
                Console.WriteLine("축하합니다!!");
                Console.WriteLine($"{dungeonName}을(를) 클리어 하였습니다.\n");

                Console.WriteLine("[탐험 결과]");
                Console.WriteLine($"체력 {beforeHp} -> {player.Hp}");
                Console.WriteLine($"Gold {beforeGold} G -> {player.Gold} G ( +{totalGold} G )");
            }

            Console.WriteLine("\n0. 나가기");
            Console.Write("\n원하시는 행동을 입력해주세요.\n>> ");
            Console.ReadLine();
        }
    }

    public class Character
    {
        public string CharacterName { get; private set; }
        public string Job { get; private set; }
        public int Level { get; private set; } = 1;
        public int BaseAtk { get; private set; } = 10;
        public int BaseDef { get; private set; } = 5;
        public int BaseHp { get; private set; } = 100;
        public int Gold { get; set; } = 1500;

        private List<Item> inventory = new List<Item>();
        private int dungeonClearCount = 0;
        public int Hp { get; private set; }

        public Character(string name, string job)
        {
            CharacterName = name;
            Job = job;
            Hp = BaseHp;
        }

        public void AddItem(Item item) => inventory.Add(item);
        public bool HasItem(Item item) => inventory.Contains(item);
        public int GetBonusAtk() => inventory.Where(i => i.IsEquipped).Sum(i => i.Atk);
        public int GetBonusDef() => inventory.Where(i => i.IsEquipped).Sum(i => i.Def);

        public void RestoreHp() => Hp = BaseHp;

        public void ReduceHp(int amount)
        {
            Hp -= amount;
            if (Hp < 0) Hp = 0;
        }
        public void ChangeJob(string newJob)
        {
            Job = newJob;
            BaseAtk += 5;
            BaseDef += 3;
            Console.WriteLine("공격력 +5 / 방어력 +3 증가!");
        }

        public void AddDungeonClear()
        {
            dungeonClearCount++;
            if (dungeonClearCount >= Level && Level < 5)
            {
                Level++;
                BaseAtk += 1;
                BaseDef += 1;
                dungeonClearCount = 0;
                Console.WriteLine($"\n[레벨업!] 레벨이 상승했습니다! Lv.{Level}");
                Console.WriteLine("공격력 +1 / 방어력 +1 증가!");
            }
        }

        public void OpenStatus()
        {
            Console.Clear();
            int bonusAtk = GetBonusAtk();
            int bonusDef = GetBonusDef();
            int totalAtk = BaseAtk + bonusAtk;
            int totalDef = BaseDef + bonusDef;

            Console.WriteLine("상태 보기");
            Console.WriteLine("캐릭터의 정보가 표시됩니다.\n");
            Console.WriteLine($"Lv. {Level}");
            Console.WriteLine($"{CharacterName} ( {Job} )");
            Console.WriteLine($"공격력\t : {totalAtk} {(bonusAtk > 0 ? $"(+{bonusAtk})" : "")}");
            Console.WriteLine($"방어력\t : {totalDef} {(bonusDef > 0 ? $"(+{bonusDef})" : "")}");
            Console.WriteLine($"체 력\t : {Hp} / {BaseHp}");
            Console.WriteLine($"Gold\t : {Gold} G");

            Console.WriteLine("\n0. 나가기");
            Console.Write(">> ");
            Console.ReadLine();
        }

        public void Inventory()
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("인벤토리\n보유 중인 아이템을 관리할 수 있습니다.\n");
                Console.WriteLine("[아이템 목록]");

                if (inventory.Count == 0)
                {
                    Console.WriteLine("- 아이템이 없습니다.");
                }
                else
                {
                    foreach (var item in inventory)
                    {
                        string equipped = item.IsEquipped ? "[E]" : "   ";
                        Console.WriteLine($"- {equipped}{item.Name,-12}\t | {item.GetEffect(),-10}\t | {item.Description}");
                    }
                }

                Console.WriteLine("\n1. 장착 관리");
                Console.WriteLine("0. 나가기");
                Console.Write("\n>> ");
                string input = Console.ReadLine();

                switch (input)
                {
                    case "1": EquipItems(); break;
                    case "0": return;
                    default:
                        Console.WriteLine("잘못된 입력입니다.");
                        Console.ReadKey();
                        break;
                }
            }
        }

        public void EquipItems()
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("인벤토리 - 장착 관리\n보유 중인 아이템을 관리할 수 있습니다.\n");

                for (int i = 0; i < inventory.Count; i++)
                {
                    var item = inventory[i];
                    string equipped = item.IsEquipped ? "[E]" : "   ";
                    Console.WriteLine($"- {i + 1}. {equipped}{item.Name,-12}\t | {item.GetEffect(),-10}\t | {item.Description}");
                }

                Console.WriteLine("\n번호를 입력하여 장착/해제를 선택하세요.");
                Console.WriteLine("0. 나가기");
                Console.Write(">> ");
                string input = Console.ReadLine();

                if (input == "0") break;

                if (int.TryParse(input, out int index) && index >= 1 && index <= inventory.Count)
                {
                    Item selectedItem = inventory[index - 1];

                    if (selectedItem.Type == ItemType.Consumable)
                    {
                        Console.WriteLine("소모품은 장착할 수 없습니다.");
                        Console.ReadKey();
                        continue;
                    }

                    if (selectedItem.IsEquipped)
                    {
                        selectedItem.IsEquipped = false;
                        Console.WriteLine($"{selectedItem.Name}을(를) 해제했습니다.");
                    }
                    else
                    {
                        if (selectedItem.Type == ItemType.Weapon || selectedItem.Type == ItemType.Armor)
                        {
                            foreach (var item in inventory)
                            {
                                if (item.IsEquipped && item.Type == selectedItem.Type)
                                {
                                    item.IsEquipped = false;
                                }
                            }
                        }

                        selectedItem.IsEquipped = true;
                        Console.WriteLine($"{selectedItem.Name}을(를) 장착했습니다.");
                    }

                    Console.ReadKey();
                }
                else
                {
                    Console.WriteLine("잘못된 입력입니다.");
                    Console.ReadKey();
                }
            }
        }
    }

    public class Shop
    {
        private Character player;
        private List<Item> ShopItems;
        private List<Item> PurchasedItems;

        public Shop(Character character)
        {
            player = character;
            PurchasedItems = new List<Item>();

            ShopItems = new List<Item>()
            {
                new Item("가죽 갑옷", 0, 3, "도적이나 사냥꾼들이 즐겨 입는 갑옷이다.", 600, ItemType.Armor),
                new Item("사슬 갑옷", 0, 6, "수련에 도움을 주는 갑옷입니다.", 1500, ItemType.Armor),
                new Item("판금 흉갑", 0, 9, "무쇠로 만들어져 튼튼한 흉갑입니다.", 3000, ItemType.Armor),
                new Item("풀 플레이트 갑옷", 0, 15, "관절부분이 유연하게 움직일 수 있게 개선된 제국 기사단제 전신 갑옷입니다.", 5000, ItemType.Armor),
                new Item("낡은 검", 2, 0, "쉽게 볼 수 있는 낡은 검 입니다.", 600,ItemType.Weapon),
                new Item("바이킹 도끼", 5, 0, "야만인들이 주로 사용했던 투박하고 무거운 도끼입니다.", 1300,ItemType.Weapon),
                new Item("균형잡힌 롱소드", 11, 0, "기사가 쓸 법한 롱소드입니다.", 2800,ItemType.Weapon),
                new Item("미사메나츠", 15, 0, "살인귀가 쓰던 검으로 피를 갈망하는 저주가 서려있다.", 4000,ItemType.Weapon),
                new Item("롱기누스", 20, 0, "신조차 죽일 수 있는 전설의 창입니다.", 10000,ItemType.Weapon),
                new Item("운명의 나침반", 2, 2, "축복이 걸린 아이템입니다. '길을 잃은 자에게 희망을'", 3000, ItemType.Accessory),
                new Item("불패의 기", 5, 5, "전란의 시대 정복왕이 사용했던 깃발", 8000, ItemType.Accessory)
            };
        }

        public void OpenShop()
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("상점\n필요한 아이템을 얻을 수 있는 상점입니다.\n");
                Console.WriteLine("[보유 골드]");
                Console.WriteLine($"{player.Gold} G\n");

                Console.WriteLine("[아이템 목록]");
                for (int i = 0; i < ShopItems.Count; i++)
                {
                    Item item = ShopItems[i];
                    string status = player.HasItem(item) ? "구매완료" : $"{item.Price} G";
                    Console.WriteLine($"- {item.Name}\t | {item.GetEffect()}\t | {item.Description}\t |  {status}");
                }

                Console.WriteLine("\n1. 아이템 구매");
                Console.WriteLine("0. 나가기");
                Console.Write(">> ");

                string input = Console.ReadLine();
                if (input == "0") break;
                else if (input == "1") PurchaseItem();
                else
                {
                    Console.WriteLine("잘못된 입력입니다.");
                    Console.ReadKey();
                }
            }
        }

        private void PurchaseItem()
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("상점 - 아이템 구매\n필요한 아이템을 얻을 수 있는 상점입니다.\n");
                Console.WriteLine("[보유 골드]");
                Console.WriteLine($"{player.Gold} G\n");

                Console.WriteLine("[아이템 목록]");

                for (int i = 0; i < ShopItems.Count; i++)
                {
                    var item = ShopItems[i];
                    string status = player.HasItem(item) || (item.Type == ItemType.Consumable && item.Name == "기사 전직서" && player.Job == "기사") ? "구매완료" : $"{item.Price} G";
                    Console.WriteLine($"- {i + 1}. {item.Name}\t | {item.GetEffect()}\t | {item.Description}\t | {status}");
                }

                Console.WriteLine("\n0. 나가기");
                Console.Write(">> ");
                string input = Console.ReadLine();

                if (input == "0") break;

                if (int.TryParse(input, out int index) && index >= 1 && index <= ShopItems.Count)
                {
                    Item selectedItem = ShopItems[index - 1];

                    bool isConsumableAndUsed =
                        selectedItem.Type == ItemType.Consumable &&
                        selectedItem.Name == "기사 전직서" &&
                        player.Job == "기사";

                    if (player.HasItem(selectedItem) || isConsumableAndUsed)
                    {
                        Console.WriteLine("이미 구매한 아이템입니다.");
                        Console.ReadKey();
                        continue;
                    }

                    if (player.Gold < selectedItem.Price)
                    {
                        Console.WriteLine("Gold가 부족합니다.");
                        Console.ReadKey();
                        continue;
                    }                                                           
                    else // 일반 아이템 (장비, 장식품)
                    {
                        player.Gold -= selectedItem.Price;
                        player.AddItem(selectedItem);
                        Console.WriteLine($"{selectedItem.Name}을(를) 구매했습니다.");
                    }

                    Console.ReadKey();
                }
                else
                {
                    Console.WriteLine("잘못된 입력입니다.");
                    Console.ReadKey();
                }
            }
        }
        public enum ItemType
        {
            Weapon,
            Armor,
            Accessory,
        }
        public class Item
        {
            public string Name { get; private set; }       // 아이템 이름
            public int Atk { get; private set; }           // 공격력 증가 수치
            public int Def { get; private set; }           // 방어력 증가 수치
            public string Description { get; private set; } // 설명
            public int Price { get; private set; }         // 가격
            public ItemType Type { get; private set; }     // 아이템 종류
            public bool IsEquipped { get; set; }           // 장착 여부
        
        public Item(string name, int atk, int def, string desc, int price, ItemType type)
        { 
            Name = name;
            Atk = atk;
            Def = def;
            Description = desc;
            Price = price;
            Type = type;
            IsEquipped = false;
        }
        public string GetEffect()
            {
                List<string> effects = new List<string>();
                if (Atk != 0) effects.Add($"공격력 +{Atk}");
                if (Def != 0) effects.Add($"방어력 +{Def}");
                return effects.Count > 0 ? string.Join(" / ", effects) : "효과 없음";
            }
        }
    }
}


    