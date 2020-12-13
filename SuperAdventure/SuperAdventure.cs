﻿using System;
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

        private void UpdateInventoryListInUI()
        {
            dgvInventory.RowHeadersVisible = false;
            dgvInventory.ColumnCount = 2;
            dgvInventory.Columns[0].Name = "Name";
            dgvInventory.Columns[0].Width = 197;
            dgvInventory.Columns[1].Name = "Quantity";

            dgvInventory.Rows.Clear();

            foreach (InventoryItem inventoryItem in player.Inventory)
            {
                if (inventoryItem.Quantity > 0)
                {
                    dgvInventory.Rows.Add(new[] { inventoryItem.Details.Name, inventoryItem.Quantity.ToString() });
                }
            }
        }

        private void UpdateQuestListInUI()
        {
            dgvQuests.RowHeadersVisible = false;

            dgvQuests.ColumnCount = 2;
            dgvQuests.Columns[0].Name = "Name";
            dgvQuests.Columns[0].Width = 197;
            dgvQuests.Columns[1].Name = "Done?";

            dgvQuests.Rows.Clear();

            foreach (PlayerQuest playerQuest in player.Quests)
            {
                dgvQuests.Rows.Add(new[]
                { playerQuest.Details.Name, playerQuest.IsCompleted.ToString() });
            }
        }

        private void UpdateWeaponsListInUI()
        {
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
        }

        private void UpdatePotionListInUI()
        {
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
            else
            {
                cboPotions.DataSource = healingPotions;
                cboPotions.DisplayMember = "Name";
                cboPotions.ValueMember = "ID";

                cboPotions.SelectedIndex = 0;
            }
        }

        private void MoveTo(Location newLocation)
        {
            //Does the location have any required items?
            if (!player.HasRequiredItemToEnterThisLocation(newLocation))
            {
                rtbMessages.Text += $"You must have a {newLocation.ItemRequiredToEnter.Name} to enter this location. \n";
                return;
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
                bool playerAlreadyHasQuest = player.HasThisQuest(newLocation.QuestAvailableHere);
                bool playerAlreadyCompletedQuest = player.CompletedThisQuest(newLocation.QuestAvailableHere);

                // See if the player already has the quest.
                if (playerAlreadyHasQuest)
                {
                    if (!playerAlreadyCompletedQuest)
                    {
                        // Check if the player has all the items needed to complete the quest.
                        bool playerHasAllItemsToCompleteQuest = player.HasAllQuestCompletionItems(newLocation.QuestAvailableHere);

                        //The player has all items required to complete the quest.
                        if (playerHasAllItemsToCompleteQuest)
                        {
                            //Display message.
                            rtbMessages.Text += "\n";
                            rtbMessages.Text += $"You complete the '{newLocation.QuestAvailableHere.Name}' quest. \n";

                            // Remove quest items from inventory.
                            player.RemoveQuestCompletionItems(newLocation.QuestAvailableHere);
                            
                            // Give quest rewards.
                            rtbMessages.Text += "You receive \n";
                            rtbMessages.Text += $"{newLocation.QuestAvailableHere.RewardExperiencePoints} experience points \n";
                            rtbMessages.Text += $"{newLocation.QuestAvailableHere.RewardGold} gold \n";
                            rtbMessages.Text += $"{newLocation.QuestAvailableHere.RewardItem.Name} \n\n";

                            player.ExperiencePoints += newLocation.QuestAvailableHere.RewardExperiencePoints;
                            player.Gold += newLocation.QuestAvailableHere.RewardGold;

                            // Add quest reward item to invenetory.
                            player.AddItemToInventory(newLocation.QuestAvailableHere.RewardItem);

                            // Mark the quest as completed.
                            player.CompletedThisQuest(newLocation.QuestAvailableHere);
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

            // Refresh player's UserInterface lists.
            UpdateInventoryListInUI();
            UpdateQuestListInUI();
            UpdateWeaponsListInUI();
            UpdatePotionListInUI();
        }

        private void btnUseWeapon_Click(object sender, EventArgs e)
        {

        }

        private void btnUsePotion_Click(object sender, EventArgs e)
        {

        }

    }
}

