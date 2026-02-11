// Translation History Manager
class TranslationHistoryManager {
    constructor() {
        this.storageKey = 'translationHistory';
        this.maxHistoryItems = 50;
    }

    saveTranslation(sourceLanguage, targetLanguage, sourceText, translatedText, provider) {
        const history = this.getAllTranslations();
        const newId = history.length > 0 ? Math.max(...history.map(h => h.Id)) + 1 : 1;
        
        const translation = {
            Id: newId,
            Date: new Date().toISOString(),
            SourceLanguage: sourceLanguage,
            TargetLanguage: targetLanguage,
            SourceText: sourceText,
            TranslatedText: translatedText,
            Provider: provider
        };

        history.unshift(translation);
        
        // Keep only the most recent items
        if (history.length > this.maxHistoryItems) {
            history.splice(this.maxHistoryItems);
        }

        localStorage.setItem(this.storageKey, JSON.stringify(history));
        return translation;
    }

    getAllTranslations() {
        try {
            const data = localStorage.getItem(this.storageKey);
            return data ? JSON.parse(data) : [];
        } catch (error) {
            console.error('Error loading translations:', error);
            return [];
        }
    }

    deleteTranslation(id) {
        const history = this.getAllTranslations();
        const filtered = history.filter(t => t.Id !== id);
        localStorage.setItem(this.storageKey, JSON.stringify(filtered));
    }

    clearAll() {
        localStorage.removeItem(this.storageKey);
    }
}

// User Preferences Manager
class PreferencesManager {
    constructor() {
        this.keys = {
            sourceLanguage: 'sourceLanguage',
            targetLanguage: 'targetLanguage',
            apiProvider: 'apiProvider'
        };
    }

    save(sourceLanguage, targetLanguage, apiProvider) {
        try {
            localStorage.setItem(this.keys.sourceLanguage, sourceLanguage);
            localStorage.setItem(this.keys.targetLanguage, targetLanguage);
            localStorage.setItem(this.keys.apiProvider, apiProvider);
        } catch (error) {
            console.warn('Failed to save preferences:', error);
        }
    }

    load() {
        try {
            return {
                sourceLanguage: localStorage.getItem(this.keys.sourceLanguage),
                targetLanguage: localStorage.getItem(this.keys.targetLanguage),
                apiProvider: localStorage.getItem(this.keys.apiProvider)
            };
        } catch (error) {
            console.warn('Failed to load preferences:', error);
            return {};
        }
    }
}

// Main Application
class TranslatorApp {
    constructor() {
        this.historyManager = new TranslationHistoryManager();
        this.preferencesManager = new PreferencesManager();
        this.isBusy = false;
        this.keepAliveInterval = null;
        
        this.initializeElements();
        this.attachEventListeners();
        this.loadPreferences();
        this.renderHistory();
        this.startKeepAlive();
    }

    initializeElements() {
        this.sourceText = document.getElementById('sourceText');
        this.sourceLanguage = document.getElementById('sourceLanguage');
        this.targetLanguage = document.getElementById('targetLanguage');
        this.apiProvider = document.getElementById('apiProvider');
        this.translatedText = document.getElementById('translatedText');
        this.translateBtn = document.getElementById('translateBtn');
        this.clearBtn = document.getElementById('clearBtn');
        this.copyBtn = document.getElementById('copyBtn');
        this.swapLanguagesBtn = document.getElementById('swapLanguages');
        this.charCount = document.getElementById('charCount');
        this.translatedCharCount = document.getElementById('translatedCharCount');
        this.errorContainer = document.getElementById('error-container');
        this.progressContainer = document.getElementById('progress-container');
        this.historyContainer = document.getElementById('translationHistory');
    }

    attachEventListeners() {
        this.translateBtn.addEventListener('click', () => this.translate());
        this.clearBtn.addEventListener('click', () => this.clear());
        this.copyBtn.addEventListener('click', () => this.copyTranslatedText());
        this.swapLanguagesBtn.addEventListener('click', () => this.swapLanguages());
        
        this.sourceText.addEventListener('input', (e) => {
            this.charCount.textContent = e.target.value.length;
        });

        this.sourceText.addEventListener('keydown', (e) => {
            if (e.key === 'Enter' && e.ctrlKey) {
                e.preventDefault();
                this.translate();
            }
        });
    }

    loadPreferences() {
        const prefs = this.preferencesManager.load();
        
        if (prefs.sourceLanguage) {
            this.sourceLanguage.value = prefs.sourceLanguage;
        } else {
            this.sourceLanguage.value = 'auto-detect';
        }

        if (prefs.targetLanguage) {
            this.targetLanguage.value = prefs.targetLanguage;
        } else {
            this.targetLanguage.value = 'en-US';
        }

        if (prefs.apiProvider) {
            this.apiProvider.value = prefs.apiProvider;
        } else {
            this.apiProvider.value = 'azure-translator';
        }
    }

    showError(message) {
        this.errorContainer.textContent = message;
        this.errorContainer.style.display = 'block';
    }

    hideError() {
        this.errorContainer.style.display = 'none';
    }

    showProgress() {
        this.progressContainer.style.display = 'block';
        this.isBusy = true;
        this.translateBtn.disabled = true;
    }

    hideProgress() {
        this.progressContainer.style.display = 'none';
        this.isBusy = false;
        this.translateBtn.disabled = false;
    }

    async translate() {
        if (this.isBusy || !this.sourceText.value.trim()) {
            return;
        }

        this.hideError();
        this.showProgress();

        const sourceText = this.sourceText.value;
        const sourceLanguage = this.sourceLanguage.value;
        const targetLanguage = this.targetLanguage.value;
        const apiProvider = this.apiProvider.value;

        // Save preferences
        this.preferencesManager.save(sourceLanguage, targetLanguage, apiProvider);

        const requestBody = {
            Content: sourceText,
            FromLang: sourceLanguage === 'auto-detect' ? null : sourceLanguage,
            ToLang: targetLanguage
        };

        try {
            const response = await fetch(`/api/translation/${apiProvider}`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify(requestBody)
            });

            if (!response.ok) {
                const errorText = await response.text();
                throw new Error(errorText || `HTTP error! status: ${response.status}`);
            }

            const data = await response.json();
            this.translatedText.textContent = data.translatedText;
            this.translatedCharCount.textContent = data.translatedText.length;
            this.copyBtn.disabled = false;

            // Save to history
            const sourceLang = window.languageList.find(l => l.Code === sourceLanguage);
            const targetLang = window.languageList.find(l => l.Code === targetLanguage);
            const provider = window.providerList.find(p => p.ApiRoute === apiProvider);

            if (sourceLang && targetLang && provider) {
                this.historyManager.saveTranslation(
                    sourceLang,
                    targetLang,
                    sourceText,
                    data.translatedText,
                    provider
                );
                this.renderHistory();
            }

        } catch (error) {
            console.error('Translation error:', error);
            this.showError(error.message || 'An error occurred while translating the text. Please try again.');
        } finally {
            this.hideProgress();
        }
    }

    clear() {
        this.sourceText.value = '';
        this.translatedText.textContent = '';
        this.charCount.textContent = '0';
        this.translatedCharCount.textContent = '0';
        this.copyBtn.disabled = true;
        this.hideError();
    }

    copyTranslatedText() {
        const text = this.translatedText.textContent;
        if (text) {
            navigator.clipboard.writeText(text).then(() => {
                const originalText = this.copyBtn.textContent;
                this.copyBtn.textContent = 'Copied!';
                setTimeout(() => {
                    this.copyBtn.textContent = originalText;
                }, 2000);
            }).catch(err => {
                console.error('Failed to copy text:', err);
            });
        }
    }

    swapLanguages() {
        const currentSource = this.sourceLanguage.value;
        const currentTarget = this.targetLanguage.value;

        // Don't swap if source is auto-detect
        if (currentSource === 'auto-detect') {
            return;
        }

        this.sourceLanguage.value = currentTarget;
        this.targetLanguage.value = currentSource;

        // Also swap the text content
        const currentTranslated = this.translatedText.textContent;
        if (currentTranslated) {
            this.sourceText.value = currentTranslated;
            this.translatedText.textContent = '';
            this.charCount.textContent = currentTranslated.length;
            this.translatedCharCount.textContent = '0';
            this.copyBtn.disabled = true;
        }
    }

    renderHistory() {
        const translations = this.historyManager.getAllTranslations();
        
        if (translations.length === 0) {
            this.historyContainer.innerHTML = '<p>No translation history available.</p>';
            return;
        }

        let html = '<div class="history-container mb-3"><ul class="translation-list">';
        
        translations.forEach(translation => {
            const date = new Date(translation.Date).toLocaleString();
            html += `
                <li>
                    <fluent-card class="translation-item mb-3">
                        <div class="hs-date mb-2">${date}, ${translation.Provider.Name}</div>
                        <div><strong>${translation.SourceLanguage.Name}</strong></div>
                        <div class="mb-2">${this.escapeHtml(this.truncateText(translation.SourceText))}</div>
                        <div><strong>${translation.TargetLanguage.Name}</strong></div>
                        <div class="mb-2">${this.escapeHtml(this.truncateText(translation.TranslatedText))}</div>
                        <fluent-button appearance="outline" class="me-2" onclick="app.loadTranslation(${translation.Id})">Load</fluent-button>
                        <fluent-button appearance="outline" onclick="app.deleteTranslation(${translation.Id})">Delete</fluent-button>
                    </fluent-card>
                </li>
            `;
        });

        html += '</ul></div>';
        html += '<fluent-button appearance="neutral" onclick="app.clearHistory()">Clear All History</fluent-button>';
        
        this.historyContainer.innerHTML = html;
    }

    loadTranslation(id) {
        const translations = this.historyManager.getAllTranslations();
        const translation = translations.find(t => t.Id === id);
        
        if (translation) {
            this.sourceLanguage.value = translation.SourceLanguage.Code;
            this.targetLanguage.value = translation.TargetLanguage.Code;
            this.sourceText.value = translation.SourceText;
            this.apiProvider.value = translation.Provider.ApiRoute;
            this.translatedText.textContent = translation.TranslatedText;
            
            this.charCount.textContent = translation.SourceText.length;
            this.translatedCharCount.textContent = translation.TranslatedText.length;
            this.copyBtn.disabled = false;
            this.hideError();
        }
    }

    deleteTranslation(id) {
        if (confirm('Are you sure you want to delete this translation?')) {
            this.historyManager.deleteTranslation(id);
            this.renderHistory();
        }
    }

    clearHistory() {
        if (confirm('Are you sure you want to clear all translations? This action cannot be undone.')) {
            this.historyManager.clearAll();
            this.renderHistory();
        }
    }

    escapeHtml(text) {
        const div = document.createElement('div');
        div.textContent = text;
        return div.innerHTML;
    }

    truncateText(text, maxLength = 100) {
        if (text.length <= maxLength) return text;
        return text.substring(0, maxLength) + '…';
    }

    startKeepAlive() {
        // Keep session alive every 60 seconds
        this.keepAliveInterval = setInterval(async () => {
            try {
                await fetch('/api/keepalive', { method: 'GET' });
            } catch (error) {
                console.error('Keep-alive error:', error);
            }
        }, 60000);
    }
}

// Initialize app when DOM is ready
let app;
if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', () => {
        app = new TranslatorApp();
    });
} else {
    app = new TranslatorApp();
}
