using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class StructuralAnimate : MonoBehaviour
{
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

    private List<SearchFile> baseResults;
    private List<SearchFile> currentResults;
    private string currentFilter;

    private readonly Stack<RepositoryButton> repositoryButtons = new();
    private readonly Stack<ChunkCard> chunkCards = new();
    private readonly HashSet<string> availableRepos = new();

    private bool isInitialized;
    private bool areResultsReady;

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

        DoNextResult();
        areResultsReady = true;
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


        foreach (SearchFile file in currentResults) {

            string filePath = file.fileName.text;
            string language = file.language;
            string url = file.externalWebUrl;

            foreach (Chunk chunk in file.chunks) {
                string code = chunk.content;
                int lineNumber = chunk.contentStart.lineNumber;
                string highlighted = SyntaxHighlighter.DoHighlight(code.Trim(), language);

                ChunkCard chunkCardInstance = Instantiate(chunkCardPrefab, codeContentAnchor);
                chunkCardInstance.Setup(filePath, language, lineNumber, highlighted, url);
                chunkCards.Push(chunkCardInstance);
            }
            
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

        OnSearchAllowed?.Invoke();
    }
}
