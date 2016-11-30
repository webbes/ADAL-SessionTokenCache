using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.SessionState;

namespace Scenario1 {
    public class NaiveSessionCache : TokenCache {
        private static readonly string itemsKey = "08A1A4F3-4BA3-4DC2-8ABC-736C547E175B";
        private static readonly string sessionKey = "63389DBF-0C2D-4F01-82E1-F8F21B3E0F2A";

        public static NaiveSessionCache Current {
            get {
                NaiveSessionCache retVal = null;
                var contextItems = HttpContext.Current.Items;
                lock (contextItems.SyncRoot) {
                    if (!contextItems.Contains(itemsKey)) {
                        retVal = new NaiveSessionCache();
                        contextItems.Add(itemsKey, retVal);
                    } else {
                        retVal = contextItems[itemsKey] as NaiveSessionCache;
                    }
                }

                return retVal;
            }
        }

        private NaiveSessionCache() : base() {
            this.session = HttpContext.Current.Session;
            this.AfterAccess = AfterAccessNotification;
            this.BeforeAccess = BeforeAccessNotification;
        }

        private readonly HttpSessionState session;
        private HttpSessionState Session {
            get {
                return this.session;
            }
        }

        public override void Clear() {
            base.Clear();
            lock (this.Session.SyncRoot) {
                this.Session.Remove(sessionKey);
            }
        }

        void BeforeAccessNotification(TokenCacheNotificationArgs args) {
            lock (this.Session.SyncRoot) {
                this.Deserialize((byte[])this.Session[sessionKey]);
            }
        }

        void AfterAccessNotification(TokenCacheNotificationArgs args) {
            if (this.HasStateChanged) {
                lock (this.Session.SyncRoot) {
                    this.HasStateChanged = false;
                    this.Session[sessionKey] = this.Serialize();
                }
            }
        }
    }
}