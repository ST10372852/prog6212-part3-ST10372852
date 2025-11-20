document.addEventListener('DOMContentLoaded', function () {
    // Add line
    const addLineBtn = document.getElementById('addLineBtn');
    const linesBody = document.getElementById('linesBody');
    const totalAmountEl = document.getElementById('totalAmount');

    function recalcTotals() {
        let total = 0;
        document.querySelectorAll('.claim-line-row').forEach(row => {
            const hours = parseFloat(row.querySelector('.hours-input')?.value || 0);
            const rate = parseFloat(row.querySelector('.rate-input')?.value || 0);
            const lineTotal = (hours * rate) || 0;
            row.querySelector('.line-total').textContent = lineTotal.toFixed(2);
            total += lineTotal;
        });
        totalAmountEl.textContent = total.toFixed(2);
    }

    function attachRowEvents(row) {
        row.querySelectorAll('input').forEach(inp => {
            inp.addEventListener('input', recalcTotals);
        });
        const rm = row.querySelector('.remove-line-btn');
        rm?.addEventListener('click', () => {
            row.remove();
            reindexRows();
            recalcTotals();
        });
    }

    function reindexRows() {
        // re-render name indices for model binding
        const rows = document.querySelectorAll('.claim-line-row');
        rows.forEach((row, index) => {
            row.querySelectorAll('input').forEach(input => {
                const name = input.getAttribute('name') || '';
                if (!name) return;
                const newName = name.replace(/Lines\[\d+\]/, `Lines[${index}]`);
                input.setAttribute('name', newName);
            });
        });
    }

    addLineBtn?.addEventListener('click', () => {
        const index = document.querySelectorAll('.claim-line-row').length;
        const tr = document.createElement('tr');
        tr.classList.add('claim-line-row');
        tr.innerHTML = `
            <td><input name="Lines[${index}].ModuleName" class="form-control" placeholder="Module/Activity" /></td>
            <td><input name="Lines[${index}].Hours" type="number" min="0" class="form-control hours-input" /></td>
            <td><input name="Lines[${index}].Rate" type="number" step="0.01" min="0" class="form-control rate-input" /></td>
            <td class="line-total align-middle">0.00</td>
            <td><button type="button" class="btn btn-sm btn-outline-danger remove-line-btn">&times;</button></td>
        `;
        linesBody.appendChild(tr);
        attachRowEvents(tr);
    });

    document.querySelectorAll('.claim-line-row').forEach(r => attachRowEvents(r));
    recalcTotals();

    // Upload form (prototype: collect file names)
    const fileInput = document.getElementById('fileInput');
    const selectedFiles = document.getElementById('selectedFiles');
    const attachBtn = document.getElementById('attachBtn');

    if (fileInput) {
        fileInput.addEventListener('change', () => {
            const names = Array.from(fileInput.files).map(f => f.name);
            selectedFiles.innerHTML = names.length ? `<ul>${names.map(n => `<li>${n}</li>`).join('')}</ul>` : '';
        });
    }

    if (attachBtn) {
        attachBtn.addEventListener('click', () => {
            const names = Array.from(fileInput.files).map(f => f.name);
            if (names.length === 0) {
                alert('Please select files to attach.');
                return;
            }
            // create a hidden form and submit the file names to the server action (prototype)
            const form = document.createElement('form');
            form.method = 'post';
            form.action = window.location.pathname.replace('/Upload', '/UploadFiles');
            const claimId = document.querySelector('input[name="claimId"]')?.value || '';
            const idInput = document.createElement('input');
            idInput.type = 'hidden';
            idInput.name = 'claimId';
            idInput.value = claimId;
            form.appendChild(idInput);

            names.forEach(n => {
                const i = document.createElement('input');
                i.type = 'hidden';
                i.name = 'fileNames';
                i.value = n;
                form.appendChild(i);
            });
            document.body.appendChild(form);
            form.submit();
        });
        // Drag & Drop + progress indicator for Upload page
        document.addEventListener("DOMContentLoaded", function () {
            const dropZone = document.getElementById("dropZone");
            const fileInput = document.getElementById("fileInput");
            const form = document.getElementById("uploadForm");
            const progressContainer = document.getElementById("progressContainer");
            const progressBar = document.getElementById("progressBar");

            if (dropZone) {
                dropZone.addEventListener("dragover", e => {
                    e.preventDefault();
                    dropZone.classList.add("bg-light");
                });

                dropZone.addEventListener("dragleave", () => {
                    dropZone.classList.remove("bg-light");
                });

                dropZone.addEventListener("drop", e => {
                    e.preventDefault();
                    dropZone.classList.remove("bg-light");
                    fileInput.files = e.dataTransfer.files;
                });
            }

            if (form) {
                form.addEventListener("submit", function (e) {
                    e.preventDefault();

                    const formData = new FormData(form);
                    const xhr = new XMLHttpRequest();
                    xhr.open("POST", form.action);

                    xhr.upload.addEventListener("progress", function (e) {
                        if (e.lengthComputable) {
                            progressContainer.style.display = "block";
                            const percent = (e.loaded / e.total) * 100;
                            progressBar.style.width = percent + "%";
                            progressBar.innerText = Math.round(percent) + "%";
                        }
                    });

                    xhr.onload = function () {
                        if (xhr.status === 200) {
                            location.reload();
                        } else {
                            alert("Upload failed");
                        }
                    };

                    xhr.send(formData);
                });
            }
        });

    }
});
