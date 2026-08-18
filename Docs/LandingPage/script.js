const carousel = document.querySelector("[data-carousel]");

if (carousel) {
  const track = carousel.querySelector("[data-carousel-track]");
  const slides = Array.from(track.children);
  const prevButton = carousel.querySelector("[data-carousel-prev]");
  const nextButton = carousel.querySelector("[data-carousel-next]");
  const dotsContainer = document.querySelector("[data-carousel-dots]");

  let activeIndex = 0;

  function renderDots() {
    dotsContainer.innerHTML = "";

    slides.forEach((_, index) => {
      const dot = document.createElement("button");
      dot.type = "button";
      dot.className = "carousel-dot";
      dot.setAttribute("aria-label", `Go to screenshot ${index + 1}`);

      if (index === activeIndex) {
        dot.classList.add("is-active");
      }

      dot.addEventListener("click", () => updateCarousel(index));
      dotsContainer.appendChild(dot);
    });
  }

  function updateCarousel(index) {
    activeIndex = (index + slides.length) % slides.length;
    track.style.transform = `translateX(-${activeIndex * 100}%)`;

    slides.forEach((slide, slideIndex) => {
      slide.classList.toggle("is-active", slideIndex === activeIndex);
    });

    renderDots();
  }

  prevButton.addEventListener("click", () => updateCarousel(activeIndex - 1));
  nextButton.addEventListener("click", () => updateCarousel(activeIndex + 1));

  updateCarousel(0);
}