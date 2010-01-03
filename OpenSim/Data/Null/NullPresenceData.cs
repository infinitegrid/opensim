/*
 * Copyright (c) Contributors, http://opensimulator.org/
 * See CONTRIBUTORS.TXT for a full list of copyright holders.
 *
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions are met:
 *     * Redistributions of source code must retain the above copyright
 *       notice, this list of conditions and the following disclaimer.
 *     * Redistributions in binary form must reproduce the above copyright
 *       notice, this list of conditions and the following disclaimer in the
 *       documentation and/or other materials provided with the distribution.
 *     * Neither the name of the OpenSimulator Project nor the
 *       names of its contributors may be used to endorse or promote products
 *       derived from this software without specific prior written permission.
 *
 * THIS SOFTWARE IS PROVIDED BY THE DEVELOPERS ``AS IS'' AND ANY
 * EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
 * WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
 * DISCLAIMED. IN NO EVENT SHALL THE CONTRIBUTORS BE LIABLE FOR ANY
 * DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
 * LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
 * ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
 * SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */

using System;
using System.Collections;
using System.Collections.Generic;
using OpenMetaverse;
using OpenSim.Framework;
using OpenSim.Data;

namespace OpenSim.Data.Null
{
    public class NullPresenceData : IPresenceData
    {
        Dictionary<UUID, PresenceData> m_presenceData = new Dictionary<UUID, PresenceData>();

        public NullPresenceData(string connectionString, string realm)
        {
            //Console.WriteLine("[XXX] NullRegionData constructor");
            // Let's stick in a test presence
            PresenceData p = new PresenceData();
            p.SessionID = UUID.Zero;
            p.UserID = UUID.Zero.ToString();
            p.Data = new Dictionary<string, string>();
            p.Data["Online"] = "true";
            m_presenceData.Add(UUID.Zero, p);
        }

        public bool Store(PresenceData data)
        {
            m_presenceData[data.SessionID] = data;
            return true;
        }

        public PresenceData Get(UUID sessionID)
        {
            if (m_presenceData.ContainsKey(sessionID))
                return m_presenceData[sessionID];

            return null;
        }

        public void LogoutRegionAgents(UUID regionID)
        {
            List<UUID> toBeDeleted = new List<UUID>();
            foreach (KeyValuePair<UUID, PresenceData> kvp in m_presenceData)
                if (kvp.Value.RegionID == regionID)
                    toBeDeleted.Add(kvp.Key);

            foreach (UUID u in toBeDeleted)
                m_presenceData.Remove(u);
        }

        public bool ReportAgent(UUID sessionID, UUID regionID, string position, string lookAt)
        {
            if (m_presenceData.ContainsKey(sessionID))
            {
                m_presenceData[sessionID].RegionID = regionID;
                m_presenceData[sessionID].Data["Position"] = position;
                m_presenceData[sessionID].Data["LookAt"] = lookAt;
                return true;
            }

            return false;
        }

        public bool SetHomeLocation(string userID, UUID regionID, Vector3 position, Vector3 lookAt)
        {
            bool foundone = false;
            foreach (PresenceData p in m_presenceData.Values)
            {
                if (p.UserID == userID)
                {
                    p.Data["HomeRegionID"] = regionID.ToString();
                    p.Data["HomePosition"] = position.ToString();
                    p.Data["HomeLookAt"] = lookAt.ToString();
                    foundone = true;
                }
            }

            return foundone;
        }

        public PresenceData[] Get(string field, string data)
        {
            List<PresenceData> presences = new List<PresenceData>();
            if (field == "UserID")
            {
                foreach (PresenceData p in m_presenceData.Values)
                    if (p.UserID == data)
                        presences.Add(p);
                return presences.ToArray();
            }
            else if (field == "SessionID")
            {
                UUID session = UUID.Zero;
                if (!UUID.TryParse(data, out session))
                    return presences.ToArray();

                if (m_presenceData.ContainsKey(session))
                {
                    presences.Add(m_presenceData[session]);
                    return presences.ToArray();
                }
            }
            else if (field == "RegionID")
            {
                UUID region = UUID.Zero;
                if (!UUID.TryParse(data, out region))
                    return presences.ToArray();
                foreach (PresenceData p in m_presenceData.Values)
                    if (p.RegionID == region)
                        presences.Add(p);
                return presences.ToArray();
            }
            else
            {
                foreach (PresenceData p in m_presenceData.Values)
                {
                    if (p.Data.ContainsKey(field) && p.Data[field] == data)
                        presences.Add(p);
                }
                return presences.ToArray();
            }

            return presences.ToArray();
        }

        public void Prune(string userID)
        {
            List<UUID> deleteSessions = new List<UUID>();
            int online = 0;

            foreach (KeyValuePair<UUID, PresenceData> kvp in m_presenceData)
            {
                bool on = false;
                if (bool.TryParse(kvp.Value.Data["Online"], out on) && on)
                    online++;
                else
                    deleteSessions.Add(kvp.Key);
            }
            if (online == 0 && deleteSessions.Count > 0)
                deleteSessions.RemoveAt(0);

            foreach (UUID s in deleteSessions)
                m_presenceData.Remove(s);

        }

        public bool Delete(string field, string data)
        {
            List<UUID> presences = new List<UUID>();
            if (field == "UserID")
            {
                foreach (KeyValuePair<UUID, PresenceData> p in m_presenceData)
                    if (p.Value.UserID == data)
                        presences.Add(p.Key);
            }
            else if (field == "SessionID")
            {
                UUID session = UUID.Zero;
                if (UUID.TryParse(data, out session))
                {
                    if (m_presenceData.ContainsKey(session))
                    {
                        presences.Add(session);
                    }
                }
            }
            else if (field == "RegionID")
            {
                UUID region = UUID.Zero;
                if (UUID.TryParse(data, out region))
                {
                    foreach (KeyValuePair<UUID, PresenceData> p in m_presenceData)
                        if (p.Value.RegionID == region)
                            presences.Add(p.Key);
                }
            }
            else
            {
                foreach (KeyValuePair<UUID, PresenceData> p in m_presenceData)
                {
                    if (p.Value.Data.ContainsKey(field) && p.Value.Data[field] == data)
                        presences.Add(p.Key);
                }
            }

            foreach (UUID u in presences)
                m_presenceData.Remove(u);

            if (presences.Count == 0)
                return false;

            return true;
        }

    }
}