// Example functions: connect these to backend later
function approveClaim(button) {
    const row = button.parentElement.parentElement;
    const lecturerName = row.cells[0].innerText;

    if (confirm(`Are you sure you want to approve the claim for ${lecturerName}?`)) {
        row.cells[4].innerText = 'Approved';
        row.removeChild(button); // remove approve button
        // Optional: send approval to backend via fetch()
        // fetch('/api/approve', { method: 'POST', body: JSON.stringify({id: claimId}) })
    }
}

function rejectClaim(button) {
    const row = button.parentElement.parentElement;
    const lecturerName = row.cells[0].innerText;
    const reason = prompt(`Enter reason for rejecting the claim for ${lecturerName}:`);

    if (reason !== null) {
        row.cells[4].innerText = 'Rejected';
        row.removeChild(row.querySelector('.approve-btn')); // remove approve button
        button.remove(); // remove reject button
        // Optional: send rejection to backend via fetch()
        // fetch('/api/reject', { method: 'POST', body: JSON.stringify({id: claimId, reason}) })
    }
}
