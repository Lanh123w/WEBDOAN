namespace WEBDOAN.wwwroot.js;
document.addEventListener("DOMContentLoaded", function () {
    document.querySelectorAll(".toggle-reaction").forEach(button => {
        button.addEventListener("click", function () {
            const commentId = this.dataset.id;
            const isHeart = this.dataset.type === "heart";

            fetch("/Comment/ToggleReaction", {
                method: "POST",
                headers: {
                    "Content-Type": "application/json",
                    "RequestVerificationToken": document.querySelector('input[name="__RequestVerificationToken"]')?.value
                },
                body: JSON.stringify({ commentId, isHeart })
            })
                .then(res => res.json())
                .then(data => {
                    if (data.success) {
                        const countSpan = this.querySelector(".count");
                        countSpan.textContent = data.count;
                        this.classList.toggle("btn-outline-danger");
                        this.classList.toggle("btn-danger");
                    }
                })
                .catch(err => console.error("Lỗi khi gửi reaction:", err));
        });
    });
});
