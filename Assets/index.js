const apiBase = "/api/v1/plugins/screenplayers";

const fileInput = document.getElementById("fileInput");
const uploadBtn = document.getElementById("uploadBtn");
const uploadStatus = document.getElementById("uploadStatus");
const myFilesEl = document.getElementById("myFiles");
const adminSection = document.getElementById("adminSection");
const allFilesEl = document.getElementById("allFiles");
const snackbar = document.getElementById("snackbar");

const formatDate = (value) => {
  if (!value) return "";
  const date = new Date(value);
  return Number.isNaN(date.getTime()) ? value : date.toLocaleString();
};

const setStatus = (message, isError = false) => {
  uploadStatus.textContent = message;
  uploadStatus.classList.toggle("error", isError);
};

const showSnackbar = (message) => {
  snackbar.textContent = message;
  snackbar.classList.add("show");
  setTimeout(() => {
    snackbar.classList.remove("show");
  }, 3000);
};

const getDisplayName = (file) => file.name || file.Name || "Unnamed file";
const getCreatorName = (file) => file.creatorName || file.CreatorName || "";
const getUploadedAt = (file) => file.uploadedAt || file.UploadedAt || "";
const getValidated = (file) => file.validated ?? file.Validated ?? false;
const getUrl = (file) => file.url || file.Url || "";

const renderFiles = (container, files, showCreator) => {
  container.innerHTML = "";
  if (!files.length) {
    return;
  }

  files.forEach((file) => {
    const row = document.createElement("div");
    row.className = "file-row";

    const isValidated = getValidated(file);
    const fileUrl = getUrl(file);

    const info = document.createElement("div");
    info.className = "file-info";

    const fileName = document.createElement("div");
    fileName.className = "file-name";
    fileName.textContent = getDisplayName(file);

    const badge = document.createElement("span");
    badge.className = `file-badge ${isValidated ? "validated" : "pending"}`;
    badge.textContent = isValidated ? "Validé" : "En attente";
    fileName.appendChild(badge);

    const meta = document.createElement("div");
    meta.className = "file-meta";
    const creatorName = getCreatorName(file);
    const creator = showCreator && creatorName ? `${creatorName} • ` : "";
    meta.textContent = `${creator}${formatDate(getUploadedAt(file))}`;

    info.appendChild(fileName);
    info.appendChild(meta);

    const actions = document.createElement("div");
    actions.className = "file-actions";

    if (isValidated && fileUrl) {
      const open = document.createElement("a");
      open.href = fileUrl;
      open.target = "_blank";
      open.rel = "noopener noreferrer";
      open.textContent = "Ouvrir";
      open.className = "link-button";
      actions.appendChild(open);

      const copy = document.createElement("button");
      copy.type = "button";
      copy.textContent = "Copier URL";
      copy.addEventListener("click", async () => {
        try {
          //Try modern clipboard API first
          if (navigator.clipboard && navigator.clipboard.writeText) {
            await navigator.clipboard.writeText(fileUrl);
            showSnackbar("✅ URL copiée !");
          } else {
            //Fallback for older browsers or non-HTTPS contexts
            const textarea = document.createElement("textarea");
            textarea.value = fileUrl;
            textarea.style.position = "fixed";
            textarea.style.opacity = "0";
            document.body.appendChild(textarea);
            textarea.select();
            const success = document.execCommand("copy");
            document.body.removeChild(textarea);
            if (success) {
              showSnackbar("✅ URL copiée !");
            } else {
              showSnackbar("❌ Erreur lors de la copie");
            }
          }
        } catch (err) {
          showSnackbar("❌ Erreur lors de la copie");
        }
      });
      actions.appendChild(copy);
    }

    if (showCreator && !isValidated) {
      const validate = document.createElement("button");
      validate.type = "button";
      validate.textContent = "Valider";
      validate.addEventListener("click", async () => {
        await validateFile(file.Id);
      });
      actions.appendChild(validate);
    }

    const del = document.createElement("button");
    del.type = "button";
    del.textContent = "Supprimer";
    del.addEventListener("click", async () => {
      if (!confirm(`Supprimer ${getDisplayName(file)} ?`)) return;
      await deleteFile(file.Id);
    });

    actions.appendChild(del);
    row.appendChild(info);
    row.appendChild(actions);
    container.appendChild(row);
  });
};

const getWorldTicket = () => {
  const raw = localStorage.getItem("worldTicketData");
  if (!raw) return "";
  try {
    const data = JSON.parse(raw);
    return (
      data?.token ||
      data?.worldTicket ||
      data?.worldTicketToken ||
      data?.ticket ||
      ""
    );
  } catch {
    return raw;
  }
};

const fetchJson = async (url, options = {}) => {
  const token = getWorldTicket();
  const headers = { ...(options.headers || {}) };
  if (token) headers["X-Auth-Token"] = token;

  const res = await fetch(url, { ...options, headers });
  if (!res.ok) {
    const text = await res.text();
    const error = new Error(text || res.statusText);
    error.status = res.status;
    throw error;
  }
  return res.json();
};

const loadMyFiles = async () => {
  const files = await fetchJson(`${apiBase}/myFiles`);
  renderFiles(myFilesEl, files, false);
};

const loadAllFiles = async () => {
  try {
    const files = await fetchJson(`${apiBase}/allFiles`);
    adminSection.classList.remove("hidden");
    renderFiles(allFilesEl, files, true);
  } catch (err) {
    if (err.status === 401 || err.status === 403) {
      adminSection.classList.add("hidden");
      return;
    }
    throw err;
  }
};

const uploadFile = async () => {
  if (!fileInput.files.length) {
    setStatus("Veuillez sélectionner un fichier à uploader.", true);
    return;
  }

  setStatus("Upload en cours...");
  const form = new FormData();
  form.append("file", fileInput.files[0]);

  try {
    await fetchJson(`${apiBase}/uploadFile`, {
      method: "POST",
      body: form,
    });
    fileInput.value = "";
    setStatus("Upload terminé avec succès !");
    await refreshLists();
  } catch (err) {
    setStatus(err.message || "Échec de l'upload.", true);
  }
};

const deleteFile = async (id) => {
  try {
    await fetchJson(`${apiBase}/DeleteFile?fileId=${encodeURIComponent(id)}`);
    setStatus("Fichier supprimé.");
    await refreshLists();
  } catch (err) {
    setStatus(err.message || "Échec de la suppression.", true);
  }
};

const validateFile = async (id) => {
  try {
    await fetchJson(`${apiBase}/ValidateFile?fileId=${encodeURIComponent(id)}`);
    setStatus("Fichier validé avec succès !");
    await refreshLists();
  } catch (err) {
    setStatus(err.message || "Échec de la validation.", true);
  }
};

const refreshLists = async () => {
  await loadMyFiles();
  await loadAllFiles();
};

uploadBtn.addEventListener("click", uploadFile);

refreshLists().catch((err) => {
  setStatus(err.message || "Échec du chargement des fichiers.", true);
});
