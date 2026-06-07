// Translation History Manager
class TranslationHistoryManager {
    constructor() {
        this.storageKey = 'translationHistory';
        this.maxHistoryItems = 50;
    }

    saveTranslation(sourceLanguage, targetLanguage, sourceText, translatedText, deployment) {
        const history = this.getAllTranslations();
        const newId = history.length > 0 ? Math.max(...history.map(h => h.Id)) + 1 : 1;
        
        const translation = {
            Id: newId,
            Date: new Date().toISOString(),
            SourceLanguage: sourceLanguage,
            TargetLanguage: targetLanguage,
            SourceText: sourceText,
            TranslatedText: translatedText,
            Deployment: deployment
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
            deploymentName: 'deploymentName'
        };
    }

    save(sourceLanguage, targetLanguage, deploymentName) {
        try {
            localStorage.setItem(this.keys.sourceLanguage, sourceLanguage);
            localStorage.setItem(this.keys.targetLanguage, targetLanguage);
            localStorage.setItem(this.keys.deploymentName, deploymentName);
        } catch (error) {
            console.warn('Failed to save preferences:', error);
        }
    }

    load() {
        try {
            return {
                sourceLanguage: localStorage.getItem(this.keys.sourceLanguage),
                targetLanguage: localStorage.getItem(this.keys.targetLanguage),
                deploymentName: localStorage.getItem(this.keys.deploymentName)
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

        this.initializeElements();
        this.attachEventListeners();
        this.loadPreferences();
        this.renderHistory();
    }

    initializeElements() {
        this.sourceText = document.getElementById('sourceText');
        this.sourceLanguage = document.getElementById('sourceLanguage');
        this.targetLanguage = document.getElementById('targetLanguage');
        this.deploymentName = document.getElementById('deploymentName');
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

        this.setDropdownValue(this.sourceLanguage, prefs.sourceLanguage, 'auto-detect');
        this.setDropdownValue(this.targetLanguage, prefs.targetLanguage, 'en-US');
        this.setDropdownValue(this.deploymentName, prefs.deploymentName);
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
        const deploymentName = this.deploymentName.value;

        if (!deploymentName) {
            this.showError('No Microsoft Foundry deployment is available.');
            this.hideProgress();
            return;
        }

        // Save preferences
        this.preferencesManager.save(sourceLanguage, targetLanguage, deploymentName);

        const requestBody = {
            Content: sourceText,
            FromLang: sourceLanguage === 'auto-detect' ? null : sourceLanguage,
            ToLang: targetLanguage
        };

        try {
            await this.translateStreaming(deploymentName, requestBody, sourceText, sourceLanguage, targetLanguage);

        } catch (error) {
            console.error('Translation error:', error);
            this.showError(error.message || 'An error occurred while translating the text. Please try again.');
        } finally {
            this.hideProgress();
        }
    }

    async translateStreaming(deploymentName, requestBody, sourceText, sourceLanguage, targetLanguage) {
        const response = await fetch(`/api/translation/${deploymentName}/stream`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'Accept': 'application/x-ndjson'
            },
            body: JSON.stringify(requestBody)
        });

        if (!response.ok) {
            const errorText = await response.text();
            throw new Error(errorText || `HTTP error! status: ${response.status}`);
        }

        if (!response.body) {
            throw new Error('Streaming is not supported by this browser.');
        }

        const reader = response.body.getReader();
        const decoder = new TextDecoder();
        let buffer = '';
        let translatedText = '';

        const handleLine = (line) => {
            if (!line.trim()) {
                return;
            }

            const event = JSON.parse(line);

            if (event.type === 'delta') {
                translatedText += event.text || '';
                this.translatedText.textContent = translatedText;
                this.translatedCharCount.textContent = translatedText.length;
                this.copyBtn.disabled = translatedText.length === 0;
                return;
            }

            if (event.type === 'error') {
                throw new Error(event.message || 'An error occurred while translating the text.');
            }
        };

        while (true) {
            const { value, done } = await reader.read();

            if (done) {
                break;
            }

            buffer += decoder.decode(value, { stream: true });
            const lines = buffer.split('\n');
            buffer = lines.pop() || '';

            lines.forEach(handleLine);
        }

        buffer += decoder.decode();
        handleLine(buffer);

        this.copyBtn.disabled = translatedText.length === 0;
        this.saveTranslationToHistory(sourceText, translatedText, sourceLanguage, targetLanguage, deploymentName);
    }

    saveTranslationToHistory(sourceText, translatedText, sourceLanguage, targetLanguage, deploymentName) {
        const sourceLang = window.languageList.find(l => l.Code === sourceLanguage);
        const targetLang = window.languageList.find(l => l.Code === targetLanguage);
        const deployment = window.deploymentList.find(d => d.Name === deploymentName);

        if (sourceLang && targetLang && deployment) {
            this.historyManager.saveTranslation(
                sourceLang,
                targetLang,
                sourceText,
                translatedText,
                deployment
            );
            this.renderHistory();
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

        this.setDropdownValue(this.sourceLanguage, currentTarget);
        this.setDropdownValue(this.targetLanguage, currentSource);

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
            const deployment = this.getHistoryDeployment(translation);
            html += `
                <li>
                    <div class="translation-item mb-3">
                        <div class="hs-date mb-2">${date}, ${this.escapeHtml(deployment.DisplayName)}</div>
                        <div><strong>${translation.SourceLanguage.Name}</strong></div>
                        <div class="mb-2">${this.escapeHtml(this.truncateText(translation.SourceText))}</div>
                        <div><strong>${translation.TargetLanguage.Name}</strong></div>
                        <div class="mb-2">${this.escapeHtml(this.truncateText(translation.TranslatedText))}</div>
                        <fluent-button appearance="secondary" class="me-2" onclick="app.loadTranslation(${translation.Id})">Load</fluent-button>
                        <fluent-button appearance="secondary" onclick="app.deleteTranslation(${translation.Id})">Delete</fluent-button>
                    </div>
                </li>
            `;
        });

        html += '</ul></div>';
        html += '<fluent-button appearance="secondary" onclick="app.clearHistory()">Clear All History</fluent-button>';
        
        this.historyContainer.innerHTML = html;
    }

    loadTranslation(id) {
        const translations = this.historyManager.getAllTranslations();
        const translation = translations.find(t => t.Id === id);
        
        if (translation) {
            this.setDropdownValue(this.sourceLanguage, translation.SourceLanguage.Code);
            this.setDropdownValue(this.targetLanguage, translation.TargetLanguage.Code);
            this.sourceText.value = translation.SourceText;
            this.setDropdownValue(this.deploymentName, this.getHistoryDeployment(translation).Name);
            this.translatedText.textContent = translation.TranslatedText;
            
            this.charCount.textContent = translation.SourceText.length;
            this.translatedCharCount.textContent = translation.TranslatedText.length;
            this.copyBtn.disabled = false;
            this.hideError();
        }
    }

    getHistoryDeployment(translation) {
        if (translation.Deployment) {
            return translation.Deployment;
        }

        return {
            Name: null,
            DisplayName: 'Microsoft Foundry'
        };
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

    setDropdownValue(dropdown, value, fallbackValue) {
        const optionValue = this.findDropdownOptionValue(dropdown, value)
            ?? this.findDropdownOptionValue(dropdown, fallbackValue)
            ?? dropdown.querySelector('fluent-option')?.value;

        if (!optionValue) {
            return;
        }

        try {
            dropdown.value = optionValue;
        } catch (error) {
            dropdown.setAttribute('value', optionValue);
        }
    }

    findDropdownOptionValue(dropdown, value) {
        if (!value) {
            return null;
        }

        return Array.from(dropdown.querySelectorAll('fluent-option'))
            .find(option => option.value === value)
            ?.value ?? null;
    }

    }

// Initialize app when DOM is ready
let app;
async function initializeApp() {
    await Promise.all([
        customElements.whenDefined('fluent-button'),
        customElements.whenDefined('fluent-dropdown'),
        customElements.whenDefined('fluent-listbox'),
        customElements.whenDefined('fluent-option'),
        customElements.whenDefined('fluent-textarea')
    ]);
    await new Promise(requestAnimationFrame);

    app = new TranslatorApp();
    window.app = app;
}

if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', () => {
        initializeApp();
    });
} else {
    initializeApp();
}
