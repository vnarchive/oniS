
	
	
	class StageServerFrontEnd{
	static logger = ::getLogger("StageServerFrontEnd");
	
	index=0;
	serverName=null;
	
	
	
	actionLogger=null;
	sessionStorage=null;
	userDataManager=null;
	playerEntries=null;
	entriesOfUserID=null;
	loggedoutEntries=null;
	
	worldConnection=null;
	conServer=null;
	serverAddress=null;
	awaits=null;
	
	iDungeons=null;
	
	inServerCloseProcess=false;
	timer=0;
	
	connectionChecker=null;
	
	isLocalMode=true;
	
	stages=null;
	stagesToRemove=null;
	
	
	
	
	constructor(actionLogger, sessionStorage, userDataManager) {
	this.actionLogger = ::notNull(actionLogger);
	this.sessionStorage = ::notNull(sessionStorage);
	this.userDataManager = ::notNull(userDataManager);
	
	this.index=index;
	this.serverName="dungeon-" + index;
	
	awaits=[];
	playerEntries={};
	entriesOfUserID={};
	loggedoutEntries=[];
	
	stages={};
	stagesToRemove=[];
	
	
	connectionChecker = ::ConnectionChecker(this);
	}
	
	function actionLog(session, action, params) {
	actionLogger.log(session, action, params);
	}
	
	
	
	
	
	function update(dt) {
	if (inServerCloseProcess) return;
	
	timer += dt;
	
	
	
	
	
	
	for (local i=loggedoutEntries.len()-1; i >= 0; i--) {
	if (!loggedoutEntries[i].checkConnection()) {
	loggedoutEntries.remove(i);
	}
	}
	local removeIndices=[];
	local curr=getTime();
	foreach (idx, rstage in stagesToRemove) {
	rstage.stage.update(dt);
	if (rstage.stage.getNrOfActivePlayers()) {
	removeIndices.append(idx);
	stages[rstage.stage.stageID] <- rstage.stage;
	} else 
	if (rstage.timelimit < curr) {
	removeIndices.append(idx);
	logger.info("------ stage stopped. id=" + rstage.stage.stageID + " ------
");
	rstage.stage.closeStageServer();
	}
	}
	local numRemoves=removeIndices.len();
	for (local i=numRemoves-1; i >= 0; i--) {
	stagesToRemove.remove(removeIndices[i]);
	}
	foreach (stage in stages) {
	stage.update(dt);
	
	if (stage.getNrOfActivePlayers() == 0) {
	if (stage.stageID in stages) {
	stagesToRemove.append({timelimit=curr+2000, stage=stages[stage.stageID]});
	
	
	removei;
	}
	}
	}
	}
	
	
	
	
	function getAvailableConnections() {
	local connections=array(
	loggedoutEntries.len()+playerEntries.len()+awaits.len());
	
	local c=0;
	foreach (entry in loggedoutEntries) {
	connections[c++]=entry.getConnection();
	}
	foreach (entry in playerEntries) {
	connections[c++]=entry.getConnection();
	}
	foreach (await in awaits) {
	connections[c++]=await.getConnection();
	}
	return connections;
	}
	
	
	
	
	
	
	function openStageServerFrontEnd(ip, port) {
	if (isLocalMode && ::SupportSinglePlay) {
	conServer=ShmConnectionServer();
	} else {
	conServer=KGLConnectionServer();
	}
	conServer.setListener(this);
	conServer.bind("0.0.0.0", port);
	
	serverAddress = ::InetSocketAddress(ip, port);
	
	if (!isLocalMode)
	connectionChecker.start();
	}
	
	
	
	
	function closeStageServerFrontEnd() {
	if (inServerCloseProcess) return;
	
	logger.info("closing server...");
	
	inServerCloseProcess=true;
	
	if (!isLocalMode)
	connectionChecker.stop();
	
	
	logger.info("close connection server");
	try {
	if (conServer != null) {
	conServer.close();
	conServer=null;
	serverAddress=null;
	}
	} catch (ex) {
	logger.error("failed to close ConnectoinServer", ex);
	}
	
	
	logger.info("cancel awaits");
	foreach (await in clone awaits) {
	try {
	await.cancel();
	} catch (ex) {
	logger.error("failed to cancel await", ex);
	}
	}
	
	logger.info("close entries");
	foreach (entry in clone playerEntries) {
	try {
	entry.logoutPlayerEntry(true, true);
	entry.closePlayerEntryConnection();
	} catch (ex) {
	logger.error("failed to close Entry", ex);
	}
	}
	
	logger.info("close logged out entries");
	foreach (entry in clone loggedoutEntries) {
	try {
	entry.closePlayerEntryConnection();
	} catch (ex) {
	logger.error("failed to close LoggedoutEntry", ex);
	}
	}
	
	
	logger.info("close database connection");
	try {
	if ("closeConnection" in userDataManager) {
	userDataManager.closeConnection();
	}
	} catch (ex) {
	logger.error("failed to close userDataManager", ex);
	}
	
	logger.info("server closed");
	}
	
	
	
	
	
	
	
	function getNrOfActivePlayers() {
	return playerEntries.len();
	}
	
	
	
	
	
	
	
	
	
	function loginPlayer(connection, sessionId, entranceId, stageId) {
	 ::notNull(connection);
	
	if (inServerCloseProcess) {
	return ::Left(EStageLogin.SERVER_CLOSED);
	}
	
	local stage=(stageId in stages) ? stages[stageId]:null;
	
	if (stage == null) {
	foreach (idx, rstage in stagesToRemove) {
	if (rstage.stage.stageID == stageId) {
	rstage.timelimit=getTime()+20000;
	stage=rstage.stage;
	break;
	}
	}
	if (stage == null) {
	stage = ::StageServer(null, actionLogger, sessionStorage, userDataManager, null);
	if (isLocalMode) {
	stage.setLocalMode();
	}
	stage.setupStage(1, stageId);
	stagesToRemove.append({timelimit=getTime()+20000, stage=stage});
	logger.info(" ++++++ stage started. id=" + stageId + " ++++++
");
	}
	}
	return stage.loginPlayer(connection, sessionId, entranceId);
	}
	
	function __characterNotFound() {return EStageLogin.CHARACTER_NOT_FOUND;}
	
	
	
	
	
	
	
	function _displaceDuplicatedPlayerRequest(userId, charaId) {
	if (isLocalMode) return EWorldPlayerLogin.SUCCESS;
	}
	
	
	
	
	
	
	
	function _getPartyData(userCharaData) {
	if (isLocalMode) return null;
	}
	
	
	
	
	
	function exitFromStage(entry) {
	if (inServerCloseProcess) {
	return;
	}
	
	local charaID=entry.getCharaID();
	
	local stage=_getEnteredStage(charaID);
	if (stage) {
	stage.exitFromStage(entry);
	
	
	
	
	
	if (stage.stageID in stages) {
	deletestages[stage.stageID];
	}
	}
	}
	
	
	
	
	
	
	
	function logoutPlayer(entry, logoutGame, logoutStages) {
	if (inServerCloseProcess) {
	return;
	}
	
	local charaID=entry.getCharaID();
	
	
	deleteplayerEntries[charaID];
	
	local userId=entry.getUserID();
	if (userId in entriesOfUserID) {
	deleteentriesOfUserID[userId];
	}
	
	
	loggedoutEntries.append(entry);
	
	
	if (logoutGame) {
	sessionStorage.destroy(entry.getSession().getID());
	}
	}
	
	
	
	
	
	
	
	
	
	
	function __createStageServerEntry(stage, connection, session, entranceId, charaData) {
	return ::StageServerPlayerEntry(
	stage, connection, session, entranceId, -1, charaData, userDataManager);
	}
	
	
	
	
	
	
	
	function _getEntryByCharaID(charaID) {
	return ::StageServerFrontEnd.getOr(playerEntries, charaID, null);
	}
	
	
	
	
	
	
	function _getEnteredStage(charaID) {
	foreach (val in stages) {
	if (val.getEntryByCharaID(charaID)) {
	return val;
	}
	}
	return null;
	}
	
	
	function notifyPartyLeave(party, leaverId) {
	}
	
	
	
	
	
	
	
	
	
	
	
	
	
	function sendFriendInvitation(senderID, senderName, targetID, targetName) {
	if (isLocalMode) return ::Left(EFriendInvitationWorld.UNKNOWN_ERROR);
	}
	
	
	
	
	
	
	
	function sendFriendAdd(senderID, senderName, targetID) {
	if (isLocalMode) return;
	}
	
	
	
	
	
	
	function sendFriendRemove(senderID, targetID) {
	if (isLocalMode) return;
	}
	
	
	
	
	
	
	
	
	
	function _catchSendWorldEventException(ex) {
	}
	
	
	
	
	function handleGetNrOfActivePlayers(req) {
	return ::GetNrOfActivePlayersResponse(
	req.sequence, serverName, getNrOfActivePlayers());
	}
	
	
	
	
	
	
	function handleWorldPlayerDisplace(req) {
	try {
	local userId=req.userId;
	
	if (userId in entriesOfUserID) {
	if (inServerCloseProcess) {
	return ::WorldPlayerDisplaceResponse(EWorldPlayerDisplace.IN_CLOSE_PROCESS);
	}
	
	logger.info("displace user[uid:" + userId + "]");
	
	local entry=entriesOfUserID[userId];
	local res=entry.logoutPlayerEntry(true, true);
	if (!res) {
	entry.closePlayerEntryConnection();
	
	logger.warn("failed logoutPlayerEntry in handleWorldPlayerDisplace[uid:" + req.userId + "]");
	return ::WorldPlayerDisplaceResponse(EWorldPlayerDisplace.FAILED);
	}
	
	entry.closePlayerEntryConnection();
	
	return ::WorldPlayerDisplaceResponse(EWorldPlayerDisplace.DISPLACE);
	} else {
	return ::WorldPlayerDisplaceResponse(EWorldPlayerDisplace.NON);
	}
	} catch (ex) {
	logger.error("error in handleWorldPlayerDisplace[uid:" + req.userId + "]", ex);
	return ::WorldPlayerDisplaceResponse(EWorldPlayerDisplace.FAILED);
	}
	}
	
	
	
	
	
	function handlePartyAddMemberNotice(event) {
	}
	
	
	
	
	
	function handlePartyLeaveNotice(event) {
	}
	
	
	
	
	
	function handlePartyChatNotice(event) {
	}
	
	
	
	
	
	function handleWhisperChatNotice(event) {
	}
	
	
	
	
	
	function handleInformationNotice(event) {
	try {
	foreach (entry in playerEntries) {
	entry.sendInformation(event.msg);
	}
	} catch (ex) {
	logger.error("error in handleInformationNotice[" + event + "]", ex);
	}
	}
	
	
	
	
	
	function handleFriendInvitationNotice(event) {
	}
	
	
	
	
	
	function handleFriendAddNotice(event) {
	}
	
	
	
	
	
	function handleFriendRemoveNotice(event) {
	}
	
	
	
	
	
	
	
	function connectionStateChanged(id, src, con) {
	try {
	logger.trace("# ConStateChanged > id:" + id + ", src:" + src);
	
	if (id == KGL_SOCKET_ACCEPTED) {
	if (!inServerCloseProcess) {
	awaitLogin(::NetConnection(con, ::EventTypes.Stage));
	} else {
	con.close();
	}
	}
	} catch (e) {
	logger.error("unexpected error [id:" + id + ", con:" + con + "]", e);
	}
	}
	
	
	function awaitLogin(connection) {
	awaits.append(AwaitLogin(this, connection));
	
	connection.sendEvent(::ConnectionCheckEvent);
	}
	
	function removeAwaitLogin(await) {
	 ::Arrays.remove(awaits, await);
	}
	}
	
	
	
	
	
	class StageServerFrontEnd.AwaitLogin{
	constructor(frontEnd, connection) {
	this.frontEnd = ::notNull(frontEnd);
	this.connection = ::notNull(connection);
	
	connection.addConnectionListener(this);
	connection.setEventHandler(::ClientEvent, this);
	}
	
	function getConnection() {
	return connection;
	}
	
	
	
	
	
	function handleStageLogin(req) {
	__removeAwait();
	
	foreach (field in req.info) {
	print("stageId=" + field.stageId + "
");
	return ::StageLoginResponse(
	frontEnd.loginPlayer(connection, req.sessionId, field.entranceId, field.stageId));
	}
	return ::StageLoginResponse(::Left(EStageLogin.BAD_REQUEST));
	}
	
	function cancel() {
	connection.disconnect();
	}
	
	function connectionClosed(_) {
	__removeAwait();
	}
	
	function __removeAwait() {
	connection.removeConnectionListener(this);
	frontEnd.removeAwaitLogin(this);
	}
	
	frontEnd=null;
	connection=null;
	}