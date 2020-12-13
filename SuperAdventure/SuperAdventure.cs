using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using Engine;

namespace SuperAdventure
{
    public partial class SuperAdventure : Form
    {
        private Player player;
        private Monster currentMonster;

        public SuperAdventure()
        {
            InitializeComponent();

            player = new Player(10, 10, 20, 0, 1);
            MoveTo(World.LocationByID(World.LOCATION_ID_HOME));
            player.Inventory.Add(new InventoryItem(World.ItemByID(World.ITEM_ID_RUSTY_SWORD), 1));

            lblHitPoints.Text = player.CurrentHitPoints.ToString();
            lblGold.Text = player.Gold.ToString();
            lblExperience.Text = player.ExperiencePoints.ToString();
            lblLevel.Text = player.Level.ToString();
        }

        private void SuperAdventure_Load(object sender, EventArgs e)
        {

        }

        private void btnNorth_Click(object sender, EventArgs e)
        {
            MoveTo(player.CurrentLocation.LocationToNorth);
        }

        private void btnEast_Click(object sender, EventArgs e)
        {
            MoveTo(player.CurrentLocation.LocationToEast);
        }

        private void btnSouth_Click(object sender, EventArgs e)
        {
            MoveTo(player.CurrentLocation.LocationToSouth);
        }

        private void btnWest_Click(object sender, EventArgs e)
        {
            MoveTo(player.CurrentLocation.LocationToWest);

        }

        private void btnUseWeapon_Click(object sender, EventArgs e)
        {

        }

        private void btnUsePotion_Click(object sender, EventArgs e)
        {

        }

        private void MoveTo(Location newLocation)
        {
            //Does the location have any required items?
            if (newLocation.ItemRequiredToEnter != null)
            {
                //See if the player has the required item in their inventory.
                bool playerHasRequiredItem = false;

                foreach (InventoryItem item in player.Inventory)
                {
                    if (item.Details.ID == newLocation.ItemRequiredToEnter.ID)
                    {
                        //We found the required item.
                        playerHasRequiredItem = true;
                        break;
                    }
                }
                if (!playerHasRequiredItem)
                {
                    //No item found. Access to area denied.
                    rtbMessages.Text += $"You must have a {newLocation.ItemRequiredToEnter.Name} to enter this location. \n";
                    return;
                }
            }

            //Update the player's current location.
            player.CurrentLocation = newLocation;

            //Display available movement buttons.
            btnNorth.Visible = (newLocation.LocationToNorth != null);
            btnEast.Visible = (newLocation.LocationToEast != null);
            btnSouth.Visible = (newLocation.LocationToSouth != null);
            btnWest.Visible = (newLocation.LocationToWest != null);

            //Display current location name and description.
            rtbLocation.Text = newLocation.Name + "\n";
            rtbLocation.Text += newLocation.Description + "\n";

            //Completely heal the player.
            player.CurrentHitPoints = player.MaximumHitPoints;

            //Update Hit Points in the UI.
            lblHitPoints.Text = player.CurrentHitPoints.ToString();

            //Does the location have a quest?
            if (newLocation.QuestAvailableHere != null)
            {
                // See if the player already has the quest, and if they've completed it.
                bool playerAlreadyHasQuest = false;
                bool playerAlreadyCompletedQuest = false;

                foreach (PlayerQuest playerQuest in player.Quests)
                {
                    if (playerQuest.Details.ID == newLocation.QuestAvailableHere.ID)
                    {
                        playerAlreadyHasQuest = true;

                        if (playerQuest.IsCompleted)
                        {
                            playerAlreadyCompletedQuest = true;
                        }
                    }
                }

                // See if the player already has the quest.
                if (playerAlreadyHasQuest)
                {
                    if (!playerAlreadyCompletedQuest)
                    {
                        // Check if the player has all the items needed to complete the quest.
                        bool playerHasAllItemsToCompleteQuest = true;

                        // See if player has item and enough it.
                        foreach (QuestCompletionItem questCompletionItem in newLocation.QuestAvailableHere.QuestCompletionItems)
                        {
                            bool foundItemInPlayersInventory = false;

                            foreach (InventoryItem inventoryItem in player.Inventory)
                            {
                                if (inventoryItem.Details.ID == questCompletionItem.Details.ID)
                                {
                                    foundItemInPlayersInventory = true;

                                    if (inventoryItem.Quantity < questCompletionItem.Quantity)
                                    {
                                        playerHasAllItemsToCompleteQuest = false;
                                        break;
                                    }
                                }
                                // Found item, so no need to check rest of inventory.
                                break;
                            }

                            // If we didn't find the required item, set variable and stop looking for other items.
                            if (!foundItemInPlayersInventory)
                            {
                                // Player doesn't have the item in inventory.
                                playerHasAllItemsToCompleteQuest = false;
                                break;
                            }
                        }

                        //The player has all items required to complete the quest.
                        if (playerHasAllItemsToCompleteQuest)
                        {
                            //Display message.
                            rtbMessages.Text += "\n";
                            rtbMessages.Text += $"You complete the '{newLocation.QuestAvailableHere.Name}' quest. \n";

                            // Remove quest items from inventory.
                            foreach (QuestCompletionItem questCompletionItem in newLocation.QuestAvailableHere.QuestCompletionItems)
                            {
                                foreach (InventoryItem inventoryItem in player.Inventory)
                                {
                                    if (inventoryItem.Details.ID == questCompletionItem.Details.ID)
                                    {
                                        // Subtract the quantity needed from players inventory.
                                        inventoryItem.Quantity -= questCompletionItem.Quantity;
                                        break;
                                    }
                                }
                            }

                            // Give quest rewards.
                            rtbMessages.Text += "You receive \n";
                            rtbMessages.Text += $"{newLocation.QuestAvailableHere.RewardExperiencePoints} experience points \n";
                            rtbMessages.Text += $"{newLocation.QuestAvailableHere.RewardGold} gold \n";
                            rtbMessages.Text += $"{newLocation.QuestAvailableHere.RewardItem.Name} \n\n";

                            player.ExperiencePoints += newLocation.QuestAvailableHere.RewardExperiencePoints;
                            player.Gold += newLocation.QuestAvailableHere.RewardGold;

                            // Add item to the inventory.
                            bool addedItemToPlayerInventory = false;

                            foreach (InventoryItem item in player.Inventory)
                            {
                                if (item.Details.ID == newLocation.QuestAvailableHere.RewardItem.ID)
                                {
                                    item.Quantity++;
                                    addedItemToPlayerInventory = true;
                                    break;
                                }
                            }
                            if (!addedItemToPlayerInventory)
                            {
                                player.Inventory.Add(new InventoryItem(newLocation.QuestAvailableHere.RewardItem, 1));
                            }

                            // Mark the quest as completed.
                            foreach (PlayerQuest quest in player.Quests)
                            {
                                if (quest.Details.ID == newLocation.QuestAvailableHere.ID)
                                {
                                    quest.IsCompleted = true;
                                    break;
                                }
                            }
                        }
                    }
                }
                // The player doesn't already have the quest.
                else
                {
                    rtbMessages.Text += $"You receive the {newLocation.QuestAvailableHere.Name} quest.\n";
                    rtbMessages.Text += $"{newLocation.QuestAvailableHere.Description}\n";
                    rtbMessages.Text += $"To complete it, return with:\n";
                    foreach (QuestCompletionItem qci in newLocation.QuestAvailableHere.QuestCompletionItems)
                    {
                        if (qci.Quantity == 1)
                        {
                            rtbMessages.Text += qci.Quantity.ToString() + " " + qci.Details.Name + Environment.NewLine;
                        }
                        else
                        {
                            rtbMessages.Text += qci.Quantity.ToString() + " " + qci.Details.NamePlural + Environment.NewLine;
                        }
                    }
                    rtbMessages.Text += Environment.NewLine;

                    // Add the quest to the player's quest list.
                    player.Quests.Add(new PlayerQuest(newLocation.QuestAvailableHere));
                }
            }

            // Does the location have a monster?
            if (newLocation.MonsterLivingHere != null)
            {
                rtbMessages.Text += $"You see a {newLocation.MonsterLivingHere.Name}\n";

                // Create monster from standard values.
                Monster standardMonster = World.MonsterByID(newLocation.MonsterLivingHere.ID);

                currentMonster = new Monster(standardMonster.ID, standardMonster.Name, standardMonster.MaximumDamage, standardMonster.RewardExperencePoints, standardMonster.RewardGold, standardMonster.CurrentHitPoints, standardMonster.MaximumHitPoints);

                foreach (LootItem lootItem in standardMonster.LootTable)
                {
                    currentMonster.LootTable.Add(lootItem);
                }

                cboWeapons.Visible = true;
                cboPotions.Visible = true;
                btnUseWeapon.Visible = true;
                btnUsePotion.Visible = true;
            }
            else
            {
                currentMonster = null;

                cboWeapons.Visible = false;
                cboPotions.Visible = false;
                btnUseWeapon.Visible = false;
                btnUsePotion.Visible = false;
            }

            // Refresh player's inventory list.
            dgvInventory.RowHeadersVisible = false;

            dgvInventory.ColumnCount = 2;
            dgvInventory.Columns[0].Name = "Name";
            dgvInventory.Columns[0].Width = 197;
            dgvInventory.Columns[1].Name = "Quantity";

            dgvInventory.Rows.Clear();

            foreach (InventoryItem ii in player.Inventory)
            {
                if (ii.Quantity > 0)
                {
                    dgvInventory.Rows.Add(new[] { ii.Details.Name, ii.Quantity.ToString() });
                }
            }

            // Refresh player's quest list.
            dgvQuests.RowHeadersVisible = false;

            dgvQuests.ColumnCount = 2;
            dgvQuests.Columns[0].Name = "Name";
            dgvQuests.Columns[0].Width = 197;
            dgvQuests.Columns[1].Name = "Done?";

            dgvQuests.Rows.Clear();

            foreach (PlayerQuest pq in player.Quests)
            {
                dgvQuests.Rows.Add(new[] { pq.Details.Name, pq.IsCompleted.ToString() });
            }

            // Refresh player's weapons combobox.
            List<Weapon> weapons = new List<Weapon>();

            foreach (InventoryItem inventoryItem in player.Inventory)
            {
                if (inventoryItem.Details is Weapon)
                {
                    if (inventoryItem.Quantity > 0)
                    {
                        weapons.Add((Weapon)inventoryItem.Details);
                    }
                }
            }
            if (weapons.Count == 0)
            {
                // The player doesn't have any weapons so hide the weapon cbobox and Use btn.
                cboWeapons.Visible = false;
                btnUseWeapon.Visible = false;
            }
            else
            {
                cboWeapons.DataSource = weapons;
                cboWeapons.DisplayMember = "Name";
                cboWeapons.ValueMember = "ID";

                cboWeapons.SelectedIndex = 0;
            }

            // Refresh the player's potion combobox.
            List<HealingPotion> healingPotions = new List<HealingPotion>();

            foreach (InventoryItem inventoryItem in player.Inventory)
            {
                if (inventoryItem.Details is HealingPotion)
                {
                    if (inventoryItem.Quantity > 0)
                    {
                        healingPotions.Add((HealingPotion)inventoryItem.Details);
                    }
                }
            }
            if (healingPotions.Count == 0)
            {
                // The player doesn't have any potions, so hide the potion combobox and use btn.
                cboPotions.Visible = false;
                btnUsePotion.Visible = false;
            }
        }

    }
}
