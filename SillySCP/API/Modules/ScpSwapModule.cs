using LabApi.Features.Wrappers;
using PlayerRoles;
using UnityEngine;

namespace SillySCP.API.Modules
{
    public static class ScpSwapModule
    {
        /// <summary>
        /// Replace player 1 with player 2, and player 2 with player 1
        /// </summary>
        /// <param name="player1">One of the players.</param>
        /// <param name="player2">The other player.</param>
        public static void Switch(Player player1, Player player2)
        {
            float requesterHealthDrain = player2.MaxHealth - player2.Health;
            float requesterShieldDrain = player2.MaxHumeShield - player2.HumeShield;
            Vector3 requesterPos = player2.Position;
            RoleTypeId requesterRole = player2.Role;

            float healthDrain = player1.MaxHealth - player1.Health;
            float shieldDrain = player1.MaxHumeShield - player1.HumeShield;
            Vector3 pos = player1.Position;

            player2.Role = player1.Role;
            player2.Position = pos;
            player2.Health -= healthDrain;
            player2.HumeShield -= shieldDrain;

            player1.Role = requesterRole;
            player1.Position = requesterPos;
            player1.Health -= requesterHealthDrain;
            player1.HumeShield -= requesterShieldDrain;
        }

        /// <summary>
        /// Switch to a role.
        /// </summary>
        /// <param name="player">The player to switch.</param>
        /// <param name="role">The role to switch to.</param>
        public static void Switch(Player player, RoleTypeId role)
        {
            float healthDrain = player.MaxHealth - player.Health;
            float shieldDrain = player.MaxHumeShield - player.HumeShield;
                
            player.Role = role;
            player.Health -= healthDrain;
            player.HumeShield -= shieldDrain;
        }

        /// <summary>
        /// Player 1 will get swapped to player 2's role, but player 2 won't change.
        /// </summary>
        /// <param name="modify">The player to modify.</param>
        /// <param name="data">The player to get data from.</param>
        public static void Replace(Player modify, Player data)
        {
            float healthDrain = data.MaxHealth - data.Health;
            float shieldDrain = data.MaxHumeShield - data.HumeShield;

            modify.Role = data.Role;
            modify.Health -= healthDrain;
            modify.HumeShield -= shieldDrain;
        }
    }
}