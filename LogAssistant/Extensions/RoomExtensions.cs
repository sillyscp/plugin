using LabApi.Features.Wrappers;

namespace LogAssistant.Extensions;

public static class RoomExtensions
{
    extension(Room? room)
    {
        private string GameObjectName => (room?.GameObject.name.Replace("(Clone)", "") ?? "Unknown").Trim();

        public string ShortName
        {
            get
            {
                if (room == null)
                    return "Unknown";

                return room.GameObjectName switch
                {
                    "PocketWorld" => "Pocket",
                    "Outside" => "Surface",
                    "LCZ_Cafe" => "Cafe",
                    "LCZ_Toilets" => "Toilets",
                    "LCZ_TCross" => "T-Crossing (LCZ)",
                    "LCZ_Airlock" => "Airlock",
                    "LCZ_ChkpA" => "Checkpoint A (LCZ)",
                    "LCZ_ChkpB" => "Checkpoint B (LCZ)",
                    "LCZ_Plants" => "Greenhouse",
                    "LCZ_Straight" => "Straight (LCZ)",
                    "LCZ_Armory" => "Armoury (LCZ)",
                    "LCZ_Crossing" => "Crossing (LCZ)",
                    "LCZ_Curve" => "Curve (LCZ)",
                    "LCZ_173" => "SCP-173 (LCZ)",
                    "LCZ_330" => "Candy",
                    "LCZ_372" => "SCP-372 (how?)",
                    "LCZ_914" => "SCP-914",
                    "LCZ_ClassDSpawn" => "Cells",
                    "HCZ_Nuke" => "Nuke (HCZ)",
                    "HCZ_TArmory" => "Armoury (HCZ)",
                    "HCZ_MicroHID_New" => "Micro H.I.D.",
                    "HCZ_Crossroom_Water" => "Acroamatic Abatement",
                    "HCZ_IncineratorWayside" => "Wayside Incinerator",
                    "HCZ_Testroom" => "Test room",
                    "HCZ_049" => "SCP-049",
                    "HCZ_079" => "SCP-079",
                    "HCZ_096" => "SCP-096",
                    "HCZ_106_Rework" => "SCP-106",
                    "HCZ_939" => "SCP-939",
                    "HCZ_Tesla_Rework" => "Tesla",
                    "HCZ_Curve" => "Curve (HCZ)",
                    "HCZ_Crossing" => "Crossing (HCZ)",
                    "HCZ_Intersection" => "Intersection (HCZ)",
                    "HCZ_Intersection_Junk" => "Junk Intersection",
                    "HCZ_Corner_Deep" => "Corner Deep",
                    "HCZ_Straight" => "Straight (HCZ)",
                    "HCZ_Straight_C" => "Straight C (HCZ)",
                    "HCZ_Straight_PipeRoom" => "Straight Piperoom",
                    "HCZ_Straight Variant" => "Straight Variant",
                    "HCZ_ChkpA" => "Checkpoint A (HCZ)",
                    "HCZ_ChkpB" => "Checkpoint B (HCZ)",
                    "HCZ_127" => "SCP-127",
                    "HCZ_ServerRoom" => "Server Room",
                    "HCZ_Intersection_Ramp" => "Loading bay",
                    "EZ_GateA" => "Gate A",
                    "EZ_GateB" => "Gate B",
                    "EZ_ThreeWay" => "T-Crossing (EZ)",
                    "EZ_Crossing" => "Crossing (EZ)",
                    "EZ_Curve" => "Curve (EZ)",
                    "EZ_PCs" => "PCs",
                    "EZ_upstairs" => "2-floor PCs (up)",
                    "EZ_Intercom" => "Intercom",
                    "EZ_Smallrooms2" => "Small rooms",
                    "EZ_PCs_small" => "2-floor PCs (down)",
                    "EZ_Chef" => "Chef",
                    "EZ_Endoof" => "Vent",
                    "EZ_CollapsedTunnel" => "Collapsed Tunnel (how?)",
                    "EZ_Smallrooms1" => "Conference",
                    "EZ_Straight" => "Straight (EZ)",
                    "EZ_StraightColumn" => "Straight Column",
                    "EZ_Cafeteria" => "Cafeteria (EZ)",
                    "EZ_Shelter" => "Shelter",
                    "EZ_HCZ_Checkpoint Part" => room.GameObject.transform.position.z switch
                    {
                        > 95 => "Checkpoint Hallway A (EZ)",
                        _ => "Checkpoint Hallway B (EZ)",
                    },
                    "HCZ_EZ_Checkpoint Part" => room.GameObject.transform.position.z switch
                    {
                        > 95 => "Checkpoint Hallway A (HCZ)",
                        _ => "Checkpoint Hallway B (HCZ)"
                    },
                    _ => "Unknown",
                };
            }
        }
    }
}