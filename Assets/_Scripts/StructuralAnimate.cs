using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StructuralAnimate : MonoBehaviour
{
    private const int PAGE_SIZE = 20;

    public event System.Action OnSearchAllowed;

    [SerializeField] private GraphicFader mainTitle;
    [SerializeField] private SearchSectionDisplacer displacer;
    [SerializeField] private GraphicFader[] scrollviewFaders;

    [SerializeField] private GraphicFader[] contentFaders;
    [SerializeField] private RectTransform repositoryContentAnchor;
    [SerializeField] private RectTransform codeContentAnchor;

    [SerializeField] private TextMeshProUGUI[] noContentTitles;

    [SerializeField] private RepositoryButton repositoryButtonPrefab;
    [SerializeField] private ChunkCard chunkCardPrefab;

    [SerializeField] private TextMeshProUGUI pageIndicatorText;
    [SerializeField] private Button prevPageButton;
    [SerializeField] private Button nextPageButton;
    [SerializeField] private GameObject pageControlsRoot;

    private List<SearchFile> baseResults;
    private List<SearchFile> currentResults;
    private string currentFilter;

    private readonly Stack<RepositoryButton> repositoryButtons = new();
    private readonly Stack<ChunkCard> chunkCards = new();
    private readonly HashSet<string> availableRepos = new();

    private bool isInitialized;
    private bool areResultsReady;

    private readonly List<ChunkRef> flatChunks = new();
    private int currentPage;

    public int TotalPages => Mathf.Max(1, Mathf.CeilToInt(flatChunks.Count / (float)PAGE_SIZE));

    private struct ChunkRef {
        public SearchFile file;
        public Chunk chunk;

        public ChunkRef(SearchFile file, Chunk chunk) {
            this.file = file;
            this.chunk = chunk;
        }
    }

    public void DoNextResult() {
        if (!isInitialized) {
            isInitialized = true;
            StartCoroutine(IDoInitialSetup());
        } else {
            StartCoroutine(IDoNextSetup(true));
        }
        areResultsReady = false;
    }

    public void UpdateResults(List<SearchFile> results) {
        baseResults = results;
        currentResults = new(results);

        currentFilter = string.Empty;
        RebuildChunks();
        currentPage = 0;
        areResultsReady = true;
    }

    public void FilterResults(string repo) {
        if (currentFilter == repo) {
            currentResults = new(baseResults);
            currentFilter = string.Empty;
        } else {
            currentResults = baseResults.Where((result) => result.repository == repo).ToList();
            currentFilter = repo;
        }

        RebuildChunks();
        currentPage = 0;
        DoNextResult();
        areResultsReady = true;
    }

    public void NextPage() {
        if (!areResultsReady) return;
        if (currentPage + 1 >= TotalPages) return;
        currentPage++;
        DoNextResult();
        areResultsReady = true;
        prevPageButton.interactable = false;
        nextPageButton.interactable = false;
    }

    public void PrevPage() {
        if (!areResultsReady) return;
        if (currentPage <= 0) return;
        currentPage--;
        DoNextResult();
        areResultsReady = true;
        prevPageButton.interactable = false;
        nextPageButton.interactable = false;
    }

    private IEnumerator IDoInitialSetup() {
        mainTitle.DoFade(0);
        yield return new WaitForSeconds(1);
        displacer.DoMove();
        yield return new WaitForSeconds(1);
        foreach (GraphicFader fader in scrollviewFaders) {
            fader.DoFade(1);
        }
        yield return new WaitForSeconds(1);

        StartCoroutine(IDoNextSetup(false));
    }

    private IEnumerator IDoNextSetup(bool awaitContentFadeOut) {

        if (awaitContentFadeOut) {
            foreach (GraphicFader fader in contentFaders) {
                fader.DoFade(0);
            }
            yield return new WaitForSeconds(1.5f);
        }

        while (!areResultsReady) {
            yield return null;
        }

        while (repositoryButtons.Count > 0) {
            Destroy(repositoryButtons.Pop().gameObject);
        }

        while (chunkCards.Count > 0) {
            Destroy(chunkCards.Pop().gameObject);
        }

        int start = currentPage * PAGE_SIZE;
        int end = Mathf.Min(start + PAGE_SIZE, flatChunks.Count);

        for (int i = start; i < end; i++) {
            SearchFile file = flatChunks[i].file;
            Chunk chunk = flatChunks[i].chunk;

            string filePath = file.fileName.text;
            string language = file.language;
            string url = file.externalWebUrl;
            string code = chunk.content;
            int lineNumber = chunk.contentStart.lineNumber;
            string highlighted = SyntaxHighlighter.DoHighlight(code.Trim(), language);

            ChunkCard chunkCardInstance = Instantiate(chunkCardPrefab, codeContentAnchor);
            chunkCardInstance.Setup(filePath, language, lineNumber, highlighted, url);
            chunkCards.Push(chunkCardInstance);
        }

        availableRepos.Clear();

        foreach (SearchFile file in baseResults) {
            string repo = file.repository;
            availableRepos.Add(repo);
        }

        foreach (string repo in availableRepos) {
            RepositoryButton repoButtonInstance = Instantiate(repositoryButtonPrefab, repositoryContentAnchor);
            repoButtonInstance.Setup(repo, this, currentFilter == repo);
            repositoryButtons.Push(repoButtonInstance);
        }

        yield return new WaitForSeconds(0.5f);

        foreach (GraphicFader fader in contentFaders) {
            fader.DoFade(1);
        }

        foreach (TextMeshProUGUI title in noContentTitles) {
            title.gameObject.SetActive(availableRepos.Count <= 0);
        }

        UpdatePageControls();

        OnSearchAllowed?.Invoke();
    }

    private void RebuildChunks() {
        flatChunks.Clear();
        foreach (SearchFile file in currentResults) {
            foreach (Chunk chunk in file.chunks) {
                flatChunks.Add(new ChunkRef(file, chunk));
            }
        }
    }

    private void UpdatePageControls() {
        pageIndicatorText.text = $"{currentPage + 1} / {TotalPages}";
        prevPageButton.interactable = currentPage > 0;
        nextPageButton.interactable = (currentPage + 1) < TotalPages;
        pageControlsRoot.SetActive(flatChunks.Count > 0);
    }
}
