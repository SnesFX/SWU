using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Steamworks;
using TinyJSON;
using UnityEngine;
using UnityEngine.UI;

public class SteamWorkshopUploader : MonoBehaviour
{
	public const int version = 10;

	public Text versionText;

	public Text statusText;

	public Slider progressBar;

	public RectTransform packListRoot;

	public RectTransform packListFrame;

	public GameObject packListButtonPrefab;

	public Text fatalErrorText;

	[Header("ModPack Interface")]
	public RectTransform currentItemPanel;

	public Text submitButtonText;

	public Text modPackContents;

	public RawImage modPackPreview;

	public InputField modPackName;

	public InputField modPackTitle;

	public InputField modPackPreviewFilename;

	public InputField modPackContentFolder;

	public InputField modPackChangeNotes;

	public InputField modPackDescription;

	public InputField modPackTags;

	public Dropdown modPackVisibility;

	private const string defaultFilename = "MyNewMod.workshop.json";

	private const string defaultFolderName = "MyNewMod";

	private const string relativeBasePath = "/../WorkshopContent/";

	private string basePath;

	private WorkshopModPack currentPack;

	private string currentPackFilename;

	private UGCUpdateHandle_t currentHandle = UGCUpdateHandle_t.Invalid;

	protected CallResult<CreateItemResult_t> m_itemCreated;

	protected CallResult<SubmitItemUpdateResult_t> m_itemSubmitted;

	private CallResult<NumberOfCurrentPlayers_t> m_NumberOfCurrentPlayers;

	private void Awake()
	{
		SetupDirectories();
	}

	private void Start()
	{
		versionText.text = string.Format("Steam Workshop Uploader - Build {0} --- App ID: {1}", 10, SteamManager.m_steamAppId);
		if (SteamManager.m_steamAppId == 0)
		{
			FatalError("Steam App ID isn't set! Make sure 'config.json' is placed next to the '.exe' file and contains an 'appId' entry with the correct app ID assigned.");
			return;
		}
		if (!SteamManager.Initialized)
		{
			FatalError("Steam API not initialized :(\n Make sure you have the Steam client running.");
			return;
		}
		RefreshPackList();
		RefreshCurrentModPack();
	}

	private void OnApplicationQuit()
	{
		if (currentPack != null)
		{
			OnCurrentModPackChanges();
			SaveCurrentModPack();
		}
		SteamAPI.Shutdown();
	}

	public void OnApplicationFocus()
	{
		RefreshPackList();
		if (currentPack != null)
		{
			RefreshCurrentModPack();
		}
	}

	public void Shutdown()
	{
		SteamAPI.Shutdown();
	}

	private void OnEnable()
	{
		basePath = Application.dataPath + "/../WorkshopContent/";
		UnityEngine.Debug.Log("basePath is: " + basePath);
		if (SteamManager.Initialized)
		{
			m_NumberOfCurrentPlayers = CallResult<NumberOfCurrentPlayers_t>.Create(OnNumberOfCurrentPlayers);
			m_itemCreated = CallResult<CreateItemResult_t>.Create(OnItemCreated);
			m_itemSubmitted = CallResult<SubmitItemUpdateResult_t>.Create(OnItemSubmitted);
		}
	}

	public void FatalError(string message)
	{
		fatalErrorText.text = "FATAL ERROR:\n" + message;
		fatalErrorText.gameObject.SetActive(true);
		packListFrame.gameObject.SetActive(false);
		currentItemPanel.gameObject.SetActive(false);
	}

	private void SetupDirectories()
	{
		basePath = Application.dataPath + "/../WorkshopContent/";
		if (!Directory.Exists(basePath))
		{
			Directory.CreateDirectory(basePath);
		}
	}

	public string[] GetPackFilenames()
	{
		return Directory.GetFiles(basePath, "*.workshop.json", SearchOption.TopDirectoryOnly);
	}

	public void ClearPackList()
	{
		IEnumerator enumerator = packListRoot.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				Transform transform = (Transform)enumerator.Current;
				UnityEngine.Object.Destroy(transform.gameObject);
			}
		}
		finally
		{
			IDisposable disposable;
			if ((disposable = enumerator as IDisposable) != null)
			{
				disposable.Dispose();
			}
		}
	}

	public void RefreshPackList()
	{
		ClearPackList();
		string[] packFilenames = GetPackFilenames();
		foreach (string text in packFilenames)
		{
			string fileName2 = Path.GetFileName(text);
			GameObject gameObject = UnityEngine.Object.Instantiate(packListButtonPrefab, Vector3.zero, Quaternion.identity);
			Button component = gameObject.GetComponent<Button>();
			component.transform.SetParent(packListRoot);
			component.GetComponentInChildren<Text>().text = fileName2.Replace(".workshop.json", string.Empty);
			if (component != null)
			{
				string fileName = text;
				component.onClick.AddListener(delegate
				{
					SelectModPack(fileName);
				});
			}
		}
	}

	public void RefreshCurrentModPack()
	{
		if (currentPack == null)
		{
			currentItemPanel.gameObject.SetActive(false);
			return;
		}
		currentItemPanel.gameObject.SetActive(true);
		string filename = currentPack.filename;
		submitButtonText.text = "Submit " + Path.GetFileNameWithoutExtension(filename.Replace(".workshop", string.Empty));
		modPackContents.text = JSON.Dump(currentPack, true);
		RefreshPreview();
		modPackTitle.text = currentPack.title;
		modPackPreviewFilename.text = currentPack.previewfile;
		modPackContentFolder.text = currentPack.contentfolder;
		modPackDescription.text = currentPack.description;
		modPackTags.text = string.Join(",", currentPack.tags.ToArray());
		modPackVisibility.value = currentPack.visibility;
	}

	public void SelectModPack(string filename)
	{
		if (currentPack != null)
		{
			OnCurrentModPackChanges();
			SaveCurrentModPack();
		}
		WorkshopModPack workshopModPack = WorkshopModPack.Load(filename);
		if (workshopModPack != null)
		{
			currentPack = workshopModPack;
			currentPackFilename = filename;
			RefreshCurrentModPack();
		}
	}

	public void EditModPack(string packPath)
	{
		Process.Start(packPath);
	}

	public void RefreshPreview()
	{
		string savedImageName = basePath + currentPack.previewfile;
		Texture2D texture = Utils.LoadTextureFromFile(savedImageName);
		modPackPreview.texture = texture;
	}

	public bool ValidateModPack(WorkshopModPack pack)
	{
		statusText.text = "Validating mod pack...";
		string fileName = basePath + pack.previewfile;
		FileInfo fileInfo = new FileInfo(fileName);
		if (fileInfo.Length >= 1048576)
		{
			statusText.text = "ERROR: Preview file must be <1MB!";
			return false;
		}
		return true;
	}

	public void OnCurrentModPackChanges()
	{
		OnChanges(currentPack);
		RefreshCurrentModPack();
	}

	public void OnChanges(WorkshopModPack pack)
	{
		pack.previewfile = modPackPreviewFilename.text;
		pack.title = modPackTitle.text;
		pack.description = modPackDescription.text;
		pack.tags = new List<string>(modPackTags.text.Split(','));
		pack.visibility = modPackVisibility.value;
	}

	public void AddModPack()
	{
		string text = modPackName.text;
		if (string.IsNullOrEmpty(text) || text.Contains("."))
		{
			statusText.text = "Bad modpack name: " + modPackName.text;
			return;
		}
		string filename = basePath + text + ".workshop.json";
		WorkshopModPack workshopModPack = new WorkshopModPack();
		workshopModPack.contentfolder = modPackName.text;
		workshopModPack.Save(filename);
		Directory.CreateDirectory(basePath + modPackName.text);
		RefreshPackList();
		SelectModPack(filename);
		CreateWorkshopItem();
	}

	public void SaveCurrentModPack()
	{
		if (currentPack != null && !string.IsNullOrEmpty(currentPackFilename))
		{
			currentPack.Save(currentPackFilename);
		}
	}

	public void SubmitCurrentModPack()
	{
		if (currentPack == null)
		{
			return;
		}
		OnChanges(currentPack);
		SaveCurrentModPack();
		if (ValidateModPack(currentPack))
		{
			if (string.IsNullOrEmpty(currentPack.publishedfileid))
			{
				CreateWorkshopItem();
			}
			else
			{
				UploadModPack(currentPack);
			}
		}
	}

	private void CreateWorkshopItem()
	{
		if (string.IsNullOrEmpty(currentPack.publishedfileid))
		{
			SteamAPICall_t hAPICall = SteamUGC.CreateItem(new AppId_t(SteamManager.m_steamAppId), EWorkshopFileType.k_EWorkshopFileTypeFirst);
			m_itemCreated.Set(hAPICall, OnItemCreated);
			statusText.text = "Creating new item...";
		}
	}

	private void UploadModPack(WorkshopModPack pack)
	{
		if (string.IsNullOrEmpty(currentPack.publishedfileid))
		{
			statusText.text = "ERROR: publishedfileid is empty, try creating the workshop item first...";
			return;
		}
		ulong value = ulong.Parse(pack.publishedfileid);
		UGCUpdateHandle_t handle = SteamUGC.StartItemUpdate(nPublishedFileID: new PublishedFileId_t(value), nConsumerAppId: new AppId_t(SteamManager.m_steamAppId));
		pack.changenote = modPackChangeNotes.text;
		currentHandle = handle;
		SetupModPack(handle, pack);
		SubmitModPack(handle, pack);
	}

	private void SetupModPack(UGCUpdateHandle_t handle, WorkshopModPack pack)
	{
		SteamUGC.SetItemTitle(handle, pack.title);
		SteamUGC.SetItemDescription(handle, pack.description);
		SteamUGC.SetItemVisibility(handle, (ERemoteStoragePublishedFileVisibility)pack.visibility);
		SteamUGC.SetItemContent(handle, basePath + pack.contentfolder);
		SteamUGC.SetItemPreview(handle, basePath + pack.previewfile);
		SteamUGC.SetItemMetadata(handle, pack.metadata);
		pack.ValidateTags();
		SteamUGC.SetItemTags(handle, pack.tags);
	}

	private void SubmitModPack(UGCUpdateHandle_t handle, WorkshopModPack pack)
	{
		SteamAPICall_t hAPICall = SteamUGC.SubmitItemUpdate(handle, pack.changenote);
		m_itemSubmitted.Set(hAPICall, OnItemSubmitted);
	}

	private void OnItemCreated(CreateItemResult_t callback, bool ioFailure)
	{
		if (ioFailure)
		{
			statusText.text = "Error: I/O Failure! :(";
			return;
		}
		switch (callback.m_eResult)
		{
		case EResult.k_EResultInsufficientPrivilege:
			statusText.text = "Error: Unfortunately, you're banned by the community from uploading to the workshop! Bummer. :(";
			break;
		case EResult.k_EResultTimeout:
			statusText.text = "Error: Timeout :S";
			break;
		case EResult.k_EResultNotLoggedOn:
			statusText.text = "Error: You're not logged into Steam!";
			break;
		case EResult.k_EResultBanned:
			statusText.text = "You don't have permission to upload content to this hub because they have an active VAC or Game ban.";
			break;
		case EResult.k_EResultServiceUnavailable:
			statusText.text = "The workshop server hosting the content is having issues - please retry.";
			break;
		case EResult.k_EResultInvalidParam:
			statusText.text = "One of the submission fields contains something not being accepted by that field.";
			break;
		case EResult.k_EResultAccessDenied:
			statusText.text = "There was a problem trying to save the title and description. Access was denied.";
			break;
		case EResult.k_EResultLimitExceeded:
			statusText.text = "You have exceeded your Steam Cloud quota. Remove some items and try again.";
			break;
		case EResult.k_EResultFileNotFound:
			statusText.text = "The uploaded file could not be found.";
			break;
		case EResult.k_EResultDuplicateRequest:
			statusText.text = "The file was already successfully uploaded. Please refresh.";
			break;
		case EResult.k_EResultDuplicateName:
			statusText.text = "You already have a Steam Workshop item with that name.";
			break;
		case EResult.k_EResultServiceReadOnly:
			statusText.text = "Due to a recent password or email change, you are not allowed to upload new content. Usually this restriction will expire in 5 days, but can last up to 30 days if the account has been inactive recently. ";
			break;
		}
		if (callback.m_bUserNeedsToAcceptWorkshopLegalAgreement)
		{
		}
		if (callback.m_eResult == EResult.k_EResultOK)
		{
			statusText.text = "Item creation successful! Published Item ID: " + callback.m_nPublishedFileId.ToString();
			UnityEngine.Debug.Log("Item created: Id: " + callback.m_nPublishedFileId.ToString());
			currentPack.publishedfileid = callback.m_nPublishedFileId.ToString();
		}
	}

	private void OnItemSubmitted(SubmitItemUpdateResult_t callback, bool ioFailure)
	{
		if (ioFailure)
		{
			statusText.text = "Error: I/O Failure! :(";
			return;
		}
		if (callback.m_bUserNeedsToAcceptWorkshopLegalAgreement)
		{
			statusText.text = "You need to accept the Steam Workshop legal agreement for this game before you can upload items!";
			return;
		}
		currentHandle = UGCUpdateHandle_t.Invalid;
		switch (callback.m_eResult)
		{
		case EResult.k_EResultOK:
			statusText.text = "SUCCESS! Item submitted! :D :D :D";
			break;
		case EResult.k_EResultFail:
			statusText.text = "Failed, dunno why :(";
			break;
		case EResult.k_EResultInvalidParam:
			statusText.text = "Either the provided app ID is invalid or doesn't match the consumer app ID of the item or, you have not enabled ISteamUGC for the provided app ID on the Steam Workshop Configuration App Admin page. The preview file is smaller than 16 bytes.";
			break;
		case EResult.k_EResultAccessDenied:
			statusText.text = "ERROR: The user doesn't own a license for the provided app ID.";
			break;
		case EResult.k_EResultFileNotFound:
			statusText.text = "Failed to get the workshop info for the item or failed to read the preview file.";
			break;
		case EResult.k_EResultLockingFailed:
			statusText.text = "Failed to aquire UGC Lock.";
			break;
		case EResult.k_EResultLimitExceeded:
			statusText.text = "The preview image is too large, it must be less than 1 Megabyte; or there is not enough space available on the users Steam Cloud.";
			break;
		}
	}

	private void UpdateProgressBar(UGCUpdateHandle_t handle)
	{
		ulong punBytesProcessed;
		ulong punBytesTotal;
		EItemUpdateStatus itemUpdateProgress = SteamUGC.GetItemUpdateProgress(handle, out punBytesProcessed, out punBytesTotal);
		float value = (float)punBytesProcessed / (float)punBytesTotal;
		progressBar.value = value;
		switch (itemUpdateProgress)
		{
		case EItemUpdateStatus.k_EItemUpdateStatusCommittingChanges:
			statusText.text = "Committing changes...";
			break;
		case EItemUpdateStatus.k_EItemUpdateStatusUploadingPreviewFile:
			statusText.text = "Uploading preview image...";
			break;
		case EItemUpdateStatus.k_EItemUpdateStatusUploadingContent:
			statusText.text = "Uploading content...";
			break;
		case EItemUpdateStatus.k_EItemUpdateStatusPreparingConfig:
			statusText.text = "Preparing configuration...";
			break;
		case EItemUpdateStatus.k_EItemUpdateStatusPreparingContent:
			statusText.text = "Preparing content...";
			break;
		}
	}

	private void OnNumberOfCurrentPlayers(NumberOfCurrentPlayers_t pCallback, bool bIOFailure)
	{
		if (pCallback.m_bSuccess != 1 || bIOFailure)
		{
			UnityEngine.Debug.Log("There was an error retrieving the NumberOfCurrentPlayers.");
		}
		else
		{
			UnityEngine.Debug.Log("The number of players playing your game: " + pCallback.m_cPlayers);
		}
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.F1))
		{
			SteamAPICall_t numberOfCurrentPlayers = SteamUserStats.GetNumberOfCurrentPlayers();
			m_NumberOfCurrentPlayers.Set(numberOfCurrentPlayers);
			UnityEngine.Debug.Log("Called GetNumberOfCurrentPlayers()");
		}
		if (currentHandle != UGCUpdateHandle_t.Invalid)
		{
			UpdateProgressBar(currentHandle);
		}
		else
		{
			progressBar.value = 0f;
		}
	}
}
