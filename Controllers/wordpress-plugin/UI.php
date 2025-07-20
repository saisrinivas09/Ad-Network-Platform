<?php
/**
 * Plugin Name:       Ad Network Dashboard
 * Description:       A unified dashboard for Clients to manage ad campaigns and for Hosts to browse available ads and get embed codes.
 * Version:           4.6 (Final Logic Fix)
 * Author:            Your Name
 */

if (!defined('ABSPATH')) exit;

define('AD_SERVER_API_URL', 'http://localhost:5244');

add_action('admin_menu', 'ad_plugin_create_menu');
function ad_plugin_create_menu() {
    add_menu_page('Ad Network', 'Ad Network', 'read', 'ad-network-dashboard', 'ad_plugin_dashboard_page_content', 'dashicons-networking', 25);
}

function ad_plugin_dashboard_page_content() {
    ?>
    <div class="wrap ad-network-dashboard">
        <div id="ad-login-container">
             <div class="login-card">
                <h1>Ad Network</h1>
                <p>Please log in to your account.</p>
                <div class="form-group">
                    <label for="ad-username">Username</label>
                    <input type="text" id="ad-username" class="ad-input">
                </div>
                <div class="form-group">
                    <label for="ad-password">Password</label>
                    <input type="password" id="ad-password" class="ad-input">
                </div>
                <button type="button" id="ad-login-button" class="ad-button primary">Login</button>
                <div id="login-status" class="status-message"></div>
            </div>
        </div>
        <div id="dashboard-container" style="display:none;"></div>
    </div>
    <style>
        .ad-network-dashboard { background-color: #f0f2f5; padding: 20px; margin-left: -20px; }
        #ad-login-container { display: flex; justify-content: center; align-items: center; min-height: 80vh; }
        .login-card { background: #fff; padding: 40px; border-radius: 8px; box-shadow: 0 4px 12px rgba(0,0,0,0.1); width: 100%; max-width: 400px; text-align: center; }
        .login-card h1 { font-size: 24px; margin-bottom: 10px; }
        .form-group { text-align: left; margin-bottom: 20px; }
        .form-group label { display: block; margin-bottom: 5px; font-weight: 600; }
        .ad-input, .ad-select, .ad-textarea { width: 100%; padding: 10px; border: 1px solid #ccd0d4; border-radius: 4px; box-sizing: border-box; }
        .ad-button { width: 100%; padding: 12px; border: none; border-radius: 4px; font-size: 16px; font-weight: 600; cursor: pointer; transition: background-color 0.2s; }
        .ad-button.primary { background-color: #2271b1; color: #fff; }
        .ad-button.primary:hover { background-color: #1e639a; }
        .status-message { margin-top: 15px; font-weight: 600; min-height: 1.2em; }
        .error { color: #d63638; }
        .success { color: #00a32a; }
        .dashboard-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 20px; }
        .dashboard-header h1 { font-size: 28px; margin: 0; }
        .dashboard-header .user-info { display: flex; align-items: center; }
        .card { background: #fff; padding: 25px; border-radius: 8px; box-shadow: 0 2px 4px rgba(0,0,0,0.05); margin-bottom: 25px; }
        .card h2 { margin-top: 0; padding-bottom: 15px; border-bottom: 1px solid #eee; }
        .grid-container { display: grid; grid-template-columns: 1fr 1fr; gap: 25px; }
        .campaign-list ul { list-style-type: none; padding: 0; margin: 0; }
        .campaign-list li { display: flex; justify-content: space-between; align-items: center; padding: 10px; border-bottom: 1px solid #f0f0f0; }
        .campaign-list li:last-child { border-bottom: none; }
        .delete-ad-btn { background: #d63638; color: #fff; border: none; padding: 4px 8px; font-size: 12px; border-radius: 3px; cursor: pointer; }
        .host-ads-table { width: 100%; border-collapse: collapse; }
        .host-ads-table th, .host-ads-table td { padding: 15px; text-align: left; border-bottom: 1px solid #f0f0f0; }
        .host-ads-table th { font-weight: 600; }
        .host-ads-table textarea { width: 100%; height: 100px; resize: vertical; }
    </style>
    <script>
        document.addEventListener('DOMContentLoaded', function() {
            let jwtToken = '';
            let currentUser = {};
            const API_URL = '<?php echo AD_SERVER_API_URL; ?>';

            // --- UTILITY FUNCTIONS ---
            function escapeHTML(str) {
                if (!str) return '';
                return str.replace(/[&<>"']/g, m => ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#039;' }[m]));
            }
            function showStatus(elementId, message, isError = true) {
                const el = document.getElementById(elementId);
                if (!el) return;
                el.textContent = message;
                el.className = isError ? 'status-message error' : 'status-message success';
                setTimeout(() => { if (el) el.textContent = ''; }, 5000);
            }
            async function apiFetch(endpoint, options = {}) {
                const headers = { ...options.headers, 'Authorization': 'Bearer ' + jwtToken };
                if (!(options.body instanceof FormData)) {
                    headers['Content-Type'] = 'application/json';
                }
                const response = await fetch(`${API_URL}${endpoint}`, { ...options, headers });
                if (!response.ok) {
                    const errorText = await response.text();
                    console.error("API Error:", errorText);
                    throw new Error(errorText || `Server error: ${response.statusText}`);
                }
                const contentType = response.headers.get("content-type");
                if (contentType && contentType.indexOf("application/json") !== -1) {
                    return response.json();
                }
                return response.text();
            }

            // --- LOGIN & LOGOUT ---
            document.getElementById('ad-login-button').addEventListener('click', handleLogin);
            async function handleLogin() {
                const username = document.getElementById('ad-username').value;
                const password = document.getElementById('ad-password').value;
                const loginButton = document.getElementById('ad-login-button');
                loginButton.textContent = 'Logging in...';
                loginButton.disabled = true;
                try {
                    const data = await apiFetch('/api/Auth/login', {
                        method: 'POST', body: JSON.stringify({ username, password })
                    });
                    jwtToken = data.token;
                    currentUser = { username, role: data.role };
                    document.getElementById('ad-login-container').style.display = 'none';
                    document.getElementById('dashboard-container').style.display = 'block';
                    if (data.role === 'Client') loadClientDashboard();
                    else if (data.role === 'Host') loadHostDashboard();
                    else if (data.role === 'Admin') loadAdminDashboardChooser();
                } catch (err) {
                    showStatus('login-status', `Login Error: Invalid credentials.`);
                } finally {
                    loginButton.textContent = 'Login';
                    loginButton.disabled = false;
                }
            }
            
            function handleLogout() {
                jwtToken = '';
                currentUser = {};
                document.getElementById('dashboard-container').style.display = 'none';
                document.getElementById('ad-login-container').style.display = 'flex';
                document.getElementById('ad-username').value = '';
                document.getElementById('ad-password').value = '';
                document.getElementById('dashboard-container').innerHTML = '';
            }
            
            // --- MAIN DASHBOARD RENDERERS ---
            function renderDashboardHeader(title) {
                return `
                    <div class="dashboard-header">
                        <h1>${title}</h1>
                        <div class="user-info">
                            <span>Welcome, <strong>${escapeHTML(currentUser.username)}</strong> (${currentUser.role})</span>
                            <button id="ad-logout-button" class="ad-button" style="width: auto; padding: 8px 15px; margin-left: 15px; background-color: #646970; color: #fff;">Logout</button>
                        </div>
                    </div>`;
            }

            function loadClientDashboard(containerId = 'dashboard-container') {
                const container = document.getElementById(containerId);
                let html = `<div class="grid-container"><div id="client-forms-container"></div><div id="campaigns-list-container"></div></div>`;
                if (containerId === 'dashboard-container') {
                    html = renderDashboardHeader('Client Dashboard') + html;
                }
                container.innerHTML = html;
                if (containerId === 'dashboard-container') {
                    document.getElementById('ad-logout-button').addEventListener('click', handleLogout);
                }
                fetchCampaignsAndRenderList();
            }

            function loadHostDashboard(containerId = 'dashboard-container') {
                const container = document.getElementById(containerId);
                let html = `<div class="card"><h2>Available Ads</h2><div id="ads-gallery"><p>Loading available ads...</p></div></div>`;
                if (containerId === 'dashboard-container') {
                    html = renderDashboardHeader('Host Dashboard') + html;
                }
                container.innerHTML = html;
                if (containerId === 'dashboard-container') {
                    document.getElementById('ad-logout-button').addEventListener('click', handleLogout);
                }
                fetchAllAdsForHost();
            }
            
            function loadAdminDashboardChooser() {
                const container = document.getElementById('dashboard-container');
                container.innerHTML = renderDashboardHeader('Admin Panel') + `
                    <div class="card">
                        <p>You have access to both dashboards. Please choose a view:</p>
                        <p>
                            <button id="view-client-btn" class="ad-button primary" style="width: auto; padding: 10px 20px; margin-right: 10px;">View Client Dashboard</button>
                            <button id="view-host-btn" class="ad-button" style="width: auto; padding: 10px 20px; background-color: #646970; color: #fff;">View Host Dashboard</button>
                        </p>
                    </div>
                    <div id="admin-view-container"></div>`;
                document.getElementById('ad-logout-button').addEventListener('click', handleLogout);
                document.getElementById('view-client-btn').addEventListener('click', () => loadClientDashboard('admin-view-container'));
                document.getElementById('view-host-btn').addEventListener('click', () => loadHostDashboard('admin-view-container'));
            }

            // --- FORM & LIST RENDERERS ---
            function renderClientForms(campaigns = []) {
                const formsContainer = document.getElementById('client-forms-container');
                let optionsHtml = campaigns.length > 0 ? campaigns.map(c => `<option value="${c.id}">${escapeHTML(c.name)}</option>`).join('') : '<option value="" disabled>-- Create a campaign first --</option>';
                formsContainer.innerHTML = `
                    <div class="card">
                        <h2>Create New Campaign</h2>
                        <form id="create-campaign-form">
                            <div class="form-group"><label for="campaign-name">Campaign Name</label><input type="text" id="campaign-name" class="ad-input" required></div>
                            <div class="form-group"><label for="start-date">Start Date</label><input type="date" id="start-date" class="ad-input" required></div>
                            <div class="form-group"><label for="end-date">End Date</label><input type="date" id="end-date" class="ad-input" required></div>
                            <button type="submit" class="ad-button primary">Create Campaign</button>
                            <div id="campaign-status" class="status-message"></div>
                        </form>
                    </div>
                    <div class="card">
                        <h2>Upload New Ad</h2>
                        <form id="upload-ad-form">
                            <div class="form-group"><label for="ad-campaign-id">Choose Campaign</label><select id="ad-campaign-id" class="ad-select" required ${campaigns.length === 0 ? 'disabled' : ''}>${optionsHtml}</select></div>
                            <div class="form-group"><label for="ad-type">Ad Type</label><select id="ad-type" class="ad-select" required><option value="Image">Image Ad</option><option value="Video">Video Ad</option></select></div>
                            <div class="form-group"><label for="ad-headline">Headline</label><input type="text" id="ad-headline" class="ad-input" required maxlength="100" placeholder="e.g., Summer Sale!"></div>
                            <div class="form-group"><label for="ad-body-text">Body Text (Optional)</label><textarea id="ad-body-text" class="ad-textarea" maxlength="250" placeholder="e.g., 50% off all items"></textarea></div>
                            <div class="form-group"><label for="ad-media-file">Media File</label><input type="file" id="ad-media-file" required accept="image/*,video/*"></div>
                            <button type="submit" class="ad-button primary" ${campaigns.length === 0 ? 'disabled' : ''}>Upload Ad</button>
                            <div id="ad-status" class="status-message"></div>
                        </form>
                    </div>`;
                document.getElementById('create-campaign-form').addEventListener('submit', handleCreateCampaign);
                document.getElementById('upload-ad-form').addEventListener('submit', handleUploadAd);
            }

            function renderCampaignsList(campaigns = []) {
                const listContainer = document.getElementById('campaigns-list-container');
                let html = `<div class="card campaign-list"><h2>Your Campaigns & Ads</h2>`;
                if (!campaigns || campaigns.length === 0) {
                    html += '<p>No campaigns found. Use the form to create one.</p>';
                } else {
                    html += campaigns.map(c => {
                        const adsHtml = c.ads.map(ad => {
                            const deleteButton = currentUser.role === 'Admin' 
                                ? `<button class="delete-ad-btn" data-ad-id="${ad.id}">Delete</button>` 
                                : '';
                            return `<li><span>${escapeHTML(ad.headline)} (<em>${ad.adType})</em></span> ${deleteButton}</li>`;
                        }).join('') || '<li>No ads yet.</li>';

                        return `
                        <div>
                            <h3>${escapeHTML(c.name)}</h3>
                            <p><strong>Ads:</strong> ${c.ads.length}</p>
                            <ul>${adsHtml}</ul>
                        </div>`;
                    }).join('<hr style="margin: 20px 0; border: 0; border-top: 1px solid #eee;">');
                }
                html += '</div>';
                listContainer.innerHTML = html;

                if (currentUser.role === 'Admin') {
                    listContainer.querySelectorAll('.delete-ad-btn').forEach(button => {
                        button.addEventListener('click', handleDeleteAd);
                    });
                }
            }

            // --- EVENT HANDLERS & DATA FETCHING ---
            async function handleCreateCampaign(event) {
                event.preventDefault();
                const button = event.target.querySelector('button');
                button.disabled = true; button.textContent = 'Creating...';
                const formData = { name: document.getElementById('campaign-name').value, startDate: document.getElementById('start-date').value, endDate: document.getElementById('end-date').value };
                try {
                    await apiFetch('/api/Campaigns', { method: 'POST', body: JSON.stringify(formData) });
                    showStatus('campaign-status', 'Campaign created successfully!', false);
                    document.getElementById('create-campaign-form').reset();
                    fetchCampaignsAndRenderList();
                } catch (err) {
                    showStatus('campaign-status', `Error: ${err.message}`);
                } finally {
                    button.disabled = false; button.textContent = 'Create Campaign';
                }
            }
            async function handleUploadAd(event) {
                event.preventDefault();
                const button = event.target.querySelector('button');
                button.disabled = true; button.textContent = 'Uploading...';
                const formData = new FormData();
                formData.append('CampaignId', document.getElementById('ad-campaign-id').value);
                formData.append('AdType', document.getElementById('ad-type').value);
                formData.append('Headline', document.getElementById('ad-headline').value);
                formData.append('BodyText', document.getElementById('ad-body-text').value);
                formData.append('MediaFile', document.getElementById('ad-media-file').files[0]);
                try {
                    await apiFetch('/api/Ads', { method: 'POST', body: formData });
                    showStatus('ad-status', 'Ad uploaded successfully!', false);
                    document.getElementById('upload-ad-form').reset();
                    fetchCampaignsAndRenderList();
                } catch (err) {
                    showStatus('ad-status', `Error: ${err.message}`);
                } finally {
                    button.disabled = false; button.textContent = 'Upload Ad';
                }
            }
            
            async function handleDeleteAd(event) {
                const adId = event.target.dataset.adId;
                if (!confirm(`Are you sure you want to permanently delete ad ID ${adId}? This cannot be undone.`)) {
                    return;
                }
                
                try {
                    await apiFetch(`/api/Ads/${adId}`, { method: 'DELETE' });
                    alert('Ad deleted successfully!');
                    fetchCampaignsAndRenderList(); // Refresh the list
                } catch (err) {
                    alert(`Error deleting ad: ${err.message}`);
                }
            }

            async function fetchCampaignsAndRenderList() {
                try {
                    const campaigns = await apiFetch('/api/Campaigns');
                    renderClientForms(campaigns);
                    renderCampaignsList(campaigns);
                } catch (error) {
                    document.getElementById('campaigns-list-container').innerHTML = `<div class="card"><p class="error">Could not load campaigns: ${error.message}</p></div>`;
                    renderClientForms([]);
                }
            }

            async function fetchAllAdsForHost() {
                const gallery = document.getElementById('ads-gallery');
                try {
                    const ads = await apiFetch('/api/Host/ads');
                    let html = '';
                    if (!ads || ads.length === 0) {
                        html += '<p>No ads are available in the network right now.</p>';
                    } else {
                        html += '<table class="host-ads-table"><thead><tr><th>Ad</th><th>Preview</th><th>Embed Code</th></tr></thead><tbody>';
                        ads.forEach(ad => {
                            const embedCode = `<a href="${API_URL}/track/click/${ad.id}?dest=https://YOUR_WEBSITE_URL_HERE" target="_blank"><iframe src="${API_URL}/serve/ad/${ad.id}" width="300" height="250" style="border:none; overflow:hidden;" title="${escapeHTML(ad.headline)}"></iframe></a>`;
                            html += `<tr>
                                <td><strong>${escapeHTML(ad.headline)}</strong><br/>${escapeHTML(ad.bodyText)}</td>
                                <td><iframe src="${API_URL}/serve/ad/${ad.id}" width="200" height="167" style="border:1px solid #ccc; pointer-events:none; border-radius: 4px;"></iframe></td>
                                <td><textarea readonly class="ad-input" onfocus="this.select();">${escapeHTML(embedCode)}</textarea><p class="description" style="font-size: 12px; color: #646970;">Replace <strong>YOUR_WEBSITE_URL_HERE</strong> with your landing page.</p></td>
                            </tr>`;
                        });
                        html += '</tbody></table>';
                    }
                    gallery.innerHTML = html;
                } catch (err) {
                    gallery.innerHTML = `<p class="error">Could not load ads: ${err.message}</p>`;
                }
            }
        });
    </script>
    <?php
}
