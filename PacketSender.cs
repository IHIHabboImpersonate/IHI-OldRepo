
using IHI.Server.Messenger;
using IHI.Server.SubPackets;

namespace IHI.Server.Net.Messages
{
    public class PacketSender
    {
        private User fUser;

        internal PacketSender(User User)
        {
            this.fUser = User;
        }

        /// <summary>
        /// Basic server -> client greeting.
        /// </summary>
        public void Send_Hello()
        {
            OutgoingMessage Message = new OutgoingMessage(0);
            
            this.fUser.GetConnection().SendMessage(Message);
        }
        /// <summary>
        /// Send the secret key.
        /// </summary>
        /// <param name="Key">The key to send.</param>
        public void Send_SecretKey(string Key)
        {
            OutgoingMessage Message = new OutgoingMessage(1);
            Message.AppendString(Key);

            this.fUser.GetConnection().SendMessage(Message);
        }

        /// <summary>
        /// Send data about a user.
        /// </summary>
        /// <param name="UserID">The user ID.</param>
        /// <param name="Username">The username.</param>
        /// <param name="Figure">The figure.</param>
        /// <param name="Gender">The gender.</param>
        /// <param name="Motto">The motto.</param>
        /// <param name="SwimFigure">The swim figure in string form.</param>
        public void Send_UserObject(uint UserID, string Username, string Figure, bool Gender, string Motto, string SwimFigure)
        {
            OutgoingMessage Message = new OutgoingMessage(5);

            Message.AppendUInt32(UserID);
            Message.AppendString(Username);
            Message.AppendString(Figure);
            Message.AppendString(Gender ? "M" : "F"); // True = Male, False = Female
            Message.AppendString(Motto);
            Message.AppendInt32(12); // TODO: Find out what this does.

            Message.Append(SwimFigure);
            Message.AppendBoolean(false);
            Message.AppendBoolean(true);

            this.fUser.GetConnection().SendMessage(Message);
        }
        /// <summary>
        /// Send the user credit balance.
        /// </summary>
        /// <param name="Balance"></param>
        public void Send_CreditBalance(int Balance)
        {
            OutgoingMessage Message = new OutgoingMessage(6);
            Message.Append(Balance);

            this.fUser.GetConnection().SendMessage(Message);

        }
        /// <summary>
        /// Send the information for the given subscription. (Habbo Club)
        /// </summary>
        /// <param name="SubscriptionName">The type of subscription.</param>
        /// <param name="CurrentDay">The amount of days into the month.</param>
        /// <param name="ElapsedMonths">The amount of passed months.</param>
        /// <param name="PrepaidMonths">The amount of unused months.</param>
        /// <param name="IsActive">Is the subscription active?</param>
        public void Send_UserInfo(string SubscriptionName, byte CurrentDay, ushort ElapsedMonths, ushort PrepaidMonths, bool IsActive)
        {
            OutgoingMessage Message = new OutgoingMessage(7);
            Message.AppendString(SubscriptionName);
            Message.AppendInt32(CurrentDay);
            Message.AppendInt32(ElapsedMonths);
            Message.AppendInt32(PrepaidMonths);
            Message.AppendBoolean(IsActive);

            this.fUser.GetConnection().SendMessage(Message);
        }

        /// <summary>
        /// Send the initial messenger configuration and contents.
        /// </summary>
        /// <param name="A">Unsure</param>
        /// <param name="B">Unsure</param>
        /// <param name="C">Unsure</param>
        public void Send_MessengerInit(int A, int B, int C, Category[] Categories, Friend[] Friends, uint MaxFriends)
        {
            OutgoingMessage Message = new OutgoingMessage(12);
            Message.AppendInt32(A);     // Find out
            Message.AppendInt32(B);     // Find out
            Message.AppendInt32(C);     // Find out

            Message.AppendInt32(Categories.Length);

            for (int i = 0; i < Categories.Length; i++)
            {
                Message.AppendUInt32(Categories[i].GetID());
                Message.AppendString(Categories[i].GetName());
            }

            Message.AppendInt32(Friends.Length);

            for (int i = 0; i < Friends.Length; i++)
            {
                Message.AppendObject(Friends[i]);
            }

            Message.AppendUInt32(MaxFriends);
            Message.AppendBoolean(false);       // TODO: Find out
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Categories">An array of all categories to show to the user.</param>
        /// <param name="Friends">An array of all friends to show to the user.</param>
        /// <param name="FriendUpdates">An array of all changes to the friends list to send to the user.</param>
        public void Send_FriendListUpdate(Category[] Categories, Friend[] Friends, FriendUpdate[] FriendUpdates)
        {
            OutgoingMessage Message = new OutgoingMessage(13);
            Message.AppendInt32(Categories.Length);

            for (int i = 0; i < Categories.Length; i++)
            {
                Message.AppendUInt32(Categories[i].GetID());
                Message.AppendString(Categories[i].GetName());
            }

            Message.AppendInt32(Friends.Length + FriendUpdates.Length);

            for (int i = 0; i < FriendUpdates.Length; i++)
            {
                Message.AppendObject(FriendUpdates[i]);
            }

            for (int i = 0; i < Friends.Length; i++)
            {
                Message.AppendObject(Friends[i]);
            }
        }
        public void Send_CloseConnection()
        {
            // HEADER: 18
        }
        public void Send_OpenConnection()
        {
            // HEADER: 19
        }
        public void Send_Chat()
        {
            // HEADER: 24
        }
        public void Send_Whisper()
        {
            // HEADER: 25
        }
        public void Send_Shout()
        {
            // HEADER: 26
        }
        public void Send_Users()
        {
            // HEADER: 28
        }
        public void Send_UserRemove()
        {
            // HEADER: 29
        }
        public void Send_HeightMap()
        {
            // HEADER: 31
        }
        public void Send_Objects()
        {
            // HEADER: 32
        }
        public void Send_GenericError()
        {
            // HEADER: 33
        }
        public void Send_UserUpdate()
        {
            // HEADER: 34
        }
        public void Send_UserBanned()
        {
            // HEADER: 35
        }
        public void Send_FlatAccessible()
        {
            // HEADER: 41
        }
        public void Send_YouAreController()
        {
            // HEADER: 42
        }
        public void Send_YouAreNotController()
        {
            // HEADER: 43
        }
        public void Send_NoSuchFlat()
        {
            // HEADER: 44
        }
        public void Send_Items()
        {
            // HEADER: 45
        }
        public void Send_RoomProperty()
        {
            // HEADER: 46
        }
        public void Send_YouAreOwner()
        {
            // HEADER: 47
        }
        public void Send_ItemDataUpdate()
        {
            // HEADER: 48
        }
        public void Send_Ping()
        {
            // HEADER: 50
        }
        public void Send_FlatCreated()
        {
            // HEADER: 59
        }
        public void Send_DoorOtherEndDeleted()
        {
            // HEADER: 63
        }
        public void Send_DoorNotInstalled()
        {
            // HEADER: 64
        }
        public void Send_PurchaseError()
        {
            // HEADER: 65
        }
        public void Send_PurchaseOK()
        {
            // HEADER: 67
        }
        public void Send_NotEnoughBalance()
        {
            // HEADER: 68
        }
        public void Send_RoomReady()
        {
            // HEADER: 69
        }
        public void Send_ItemAdd()
        {
            // HEADER: 83
        }
        public void Send_ItemRemove()
        {
            // HEADER: 84
        }
        public void Send_ItemUpdate()
        {
            // HEADER: 85
        }
        public void Send_ObjectDataUpdate()
        {
            // HEADER: 88
        }
        public void Send_DiceValue()
        {
            // HEADER: 90
        }
        public void Send_Doorbell()
        {
            // HEADER: 91
        }
        public void Send_ObjectAdd()
        {
            // HEADER: 93
        }
        public void Send_ObjectRemove()
        {
            // HEADER: 94
        }
        public void Send_ObjectUpdate()
        {
            // HEADER: 95
        }
        public void Send_FurniListInsert()
        {
            // HEADER: 98
        }
        public void Send_FurniListRemove()
        {
            // HEADER: 99
        }
        public void Send_FurniListUpdate()
        {
            // HEADER: 101
        }
        public void Send_TradingYouAreNotAllowed()
        {
            // HEADER: 102
        }
        public void Send_TradingOtherNotAllowed()
        {
            // HEADER: 103
        }
        public void Send_TradingOpen()
        {
            // HEADER: 104
        }
        public void Send_TradingAlreadyOpen()
        {
            // HEADER: 105
        }
        public void Send_TradingNotOpen()
        {
            // HEADER: 106
        }
        public void Send_TradingNoSuchItem()
        {
            // HEADER: 107
        }
        public void Send_TradingItemList()
        {
            // HEADER: 108
        }
        public void Send_TradingAccept()
        {
            // HEADER: 109
        }
        public void Send_TradingClose()
        {
            // HEADER: 110
        }
        public void Send_TradingConfirmation()
        {
            // HEADER: 111
        }
        public void Send_TradingCompleted()
        {
            // HEADER: 112
        }
        public void Send_CatalogIndex()
        {
            // HEADER: 126
        }
        public void Send_CatalogPage()
        {
            // HEADER: 127
        }
        public void Send_PresentOpened()
        {
            // HEADER: 129
        }
        public void Send_FlatAccessDenied()
        {
            // HEADER: 131
        }
        public void Send_NewBuddyRequest()
        {
            // HEADER: 132
        }
        public void Send_NewConsole()
        {
            // HEADER: 134
        }
        public void Send_RoomInvite()
        {
            // HEADER: 135
        }
        public void Send_FurniList()
        {
            // HEADER: 140
        }
        public void Send_PostItPlaced()
        {
            // HEADER: 145
        }
        public void Send_Mod()
        {
            // HEADER: 161
        }
        public void Send_VoucherRedeemOk()
        {
            // HEADER: 212
        }
        public void Send_VoucherRedeemError()
        {
            // HEADER: 213
        }
        public void Send_HeightMapUpdate()
        {
            // HEADER: 219
        }
        public void Send_UserFlatCats()
        {
            // HEADER: 221
        }
        public void Send_FlatCat()
        {
            // HEADER: 222
        }
        public void Send_HabboUserBadges()
        {
            // HEADER: 228
        }
        public void Send_Badges()
        {
            // HEADER: 229
        }
        public void Send_SlideObjectsBundle()
        {
            // HEADER: 230
        }
        public void Send_SessionParams()
        {
            // HEADER: 257
        }
        public void Send_MessengerError()
        {
            // HEADER: 260
        }
        public void Send_InstantMessageError()
        {
            // HEADER: 261
        }
        public void Send_RoomInviteError()
        {
            // HEADER: 262
        }
        public void Send_UserChange()
        {
            // HEADER: 266
        }

        /// <summary>
        /// Encryption is not supported by IHI.
        /// This is part of the no encryption setting that always seemed to work.
        /// </summary>
        public void Send_InitCrypto(string Token, bool Status)
        {
            // HEADER: 227
            OutgoingMessage Message = new OutgoingMessage(227);
            Message.AppendString(Token);
            Message.AppendBoolean(Status);

            this.fUser.GetConnection().SendMessage(Message);
        }
        public void Send_RoomForward()
        {
            // HEADER: 286
        }
        public void Send_DisconnectReason()
        {
            // HEADER: 287
        }
        public void Send_PurchaseNotAllowed()
        {
            // HEADER: 296
        }
        public void Send_ErrorReport()
        {
            // HEADER: 299
        }
        public void Send_HabboGroupsBadges()
        {
            // HEADER: 309
        }
        public void Send_OneWayDoorStatus()
        {
            // HEADER: 312
        }
        public void Send_FriendRequests(uint[] UserIDs, string[] Usernames)
        {
            // TODO: Add this handler
            // HEADER: 314
        }
        public void Send_AcceptBuddyResult()
        {
            // HEADER: 315
        }
        public void Send_RoomRating()
        {
            // HEADER: 345
        }
        public void Send_FollowFriendFailed()
        {
            // HEADER: 349
        }
        public void Send_UserTags()
        {
            // HEADER: 350
        }
        public void Send_UserTyping()
        {
            // HEADER: 361
        }
        public void Send_RoomDimmer()
        {
            // HEADER: 365
        }
        public void Send_CanCreateRoomEvent()
        {
            // HEADER: 367
        }
        public void Send_RoomEvent()
        {
            // HEADER: 370
        }
        public void Send_IgnoreResult()
        {
            // HEADER: 419
        }
        public void Send_IgnoredUsers()
        {
            // HEADER: 420
        }
        public void Send_HabboSearchResult()
        {
            // HEADER: 435
        }
        public void Send_Achievements()
        {
            // HEADER: 436
        }
        public void Send_HabboAcievementNotification()
        {
            // HEADER: 437
        }
        public void Send_HabboActivityPointNotification()
        {
            // HEADER: 438
        }
        public void Send_UniqueMachineID()
        {
            // HEADER: 439
        }
        public void Send_RespectNotification()
        {
            // HEADER: 440
        }
        public void Send_CatalogPublished()
        {
            // HEADER: 441
        }
        public void Send_NavigatorFrontPageResult()
        {
            // HEADER: 450
        }
        public void Send_GuestRoomSearchResult()
        {
            // HEADER: 451
        }
        public void Send_PopularRoomTagsResult()
        {
            // HEADER: 452
        }
        public void Send_OfficialRoomsResult()
        {
            // HEADER: 453
        }
        public void Send_GetGuestRoomResult()
        {
            // HEADER: 454
        }
        public void Send_NavigatorSettings()
        {
            // HEADER: 455
        }
        public void Send_RoomInfoUpdated()
        {
            // HEADER: 456
        }
        public void Send_RoomThumbnailUpdateResult()
        {
            // HEADER: 457
        }
        public void Send_Favourites()
        {
            // HEADER: 458
        }
        public void Send_FavouriteChanged()
        {
            // HEADER: 459
        }
        public void Send_AvatarEffects()
        {
            // HEADER: 460
        }
        public void Send_AvatarEffectAdded()
        {
            // HEADER: 461
        }
        public void Send_AvatarEffectActivated()
        {
            // HEADER: 462
        }
        public void Send_AvatarEffectExpired()
        {
            // HEADER: 463
        }
        public void Send_AvatarEffectSelected()
        {
            // HEADER: 464
        }
        public void Send_RoomSettingsData()
        {
            // HEADER: 465
        }
        public void Send_RoomSettingsError()
        {
            // HEADER: 466
        }
        public void Send_RoomSettingsSaved()
        {
            // HEADER: 467
        }
        public void Send_RoomSettingsSaveError()
        {
            // HEADER: 468
        }
        public void Send_FloorHeightMap()
        {
            // HEADER: 470
        }
        public void Send_RoomEntryInfo()
        {
            // HEADER: 471
        }
        public void Send_Dance()
        {
            // HEADER: 480
        }
        public void Send_Wave()
        {
            // HEADER: 481
        }
        public void Send_CarryObject()
        {
            // HEADER: 482
        }
        public void Send_AvatarEffect()
        {
            // HEADER: 485
        }
        public void Send_Sleep()
        {
            // HEADER: 486
        }
        public void Send_UserObject()
        {
            // HEADER: 488
        }
        public void Send_FlatControllerAdded()
        {
            // HEADER: 510
        }
        public void Send_FlatControllerRemoved()
        {
            // HEADER: 511
        }
        public void Send_CanCreateRoom()
        {
            // HEADER: 512
        }
        public void Send_PlaceObjectError()
        {
            // HEADER: 516
        }
        public void Send_InfoFeedEnable()
        {
            // HEADER: 517
        }

    }
}
