using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System.Web;
using System.Web.SessionState;

namespace Scenario1 {
    public class SessionTokenCache : TokenCache {
        private static readonly string itemsKey = "08A1A4F3-4BA3-4DC2-8ABC-736C547E175B";
        private static readonly string sessionKey = "63389DBF-0C2D-4F01-82E1-F8F21B3E0F2A";

        public static SessionTokenCache Current {
            get {
                SessionTokenCache retVal = null;
                var contextItems = HttpContext.Current.Items;
                lock (contextItems.SyncRoot) {
                    if (!contextItems.Contains(itemsKey)) {
                        retVal = new SessionTokenCache();
                        contextItems.Add(itemsKey, retVal);
                    } else {
                        retVal = contextItems[itemsKey] as SessionTokenCache;
                    }
                }

                return retVal;
            }
        }

        private SessionTokenCache() : base() {
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