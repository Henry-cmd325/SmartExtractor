<template>
  <header class="flex justify-between items-center w-full px-8 py-4 bg-surface z-50">
    <div class="flex items-center gap-4">
      <span class="font-headline font-black tracking-tighter text-xl text-primary uppercase">Smart extractor</span>
      <div class="h-4 w-[1px] bg-outline-variant/30" />
      <h1 class="font-headline font-bold tracking-tight text-on-surface-variant text-sm uppercase">
        PDF Data Extractor: Hackathon Edition
      </h1>
    </div>
    <div class="flex items-center gap-6">
      <nav class="hidden md:flex items-center gap-8">
        <a
          class="font-headline font-bold tracking-tight text-primary border-b-2 border-primary pb-1"
          href="#"
        >Dashboard</a>
        <a
          class="font-headline font-bold tracking-tight text-on-surface-variant hover:text-primary transition-colors"
          href="#"
        >History</a>
      </nav>
      <div class="flex items-center gap-3">
        <button class="p-2 text-on-surface-variant hover:bg-surface-container-high rounded-lg transition-all active:scale-95">
          <span class="material-symbols-outlined">notifications</span>
        </button>
        <button class="p-2 text-on-surface-variant hover:bg-surface-container-high rounded-lg transition-all active:scale-95">
          <span class="material-symbols-outlined">settings</span>
        </button>
        <div class="w-8 h-8 rounded-full overflow-hidden border border-primary/20">
          <img
            alt="User Avatar"
            src="https://lh3.googleusercontent.com/aida-public/AB6AXuD4aOQMu_g7k6B6wEA856DQffqtfJ9uqztJvWZSQr_sFdBfL4N93rLLvgEsDspce1Ogz_9D190rFbHP-u5Z_llCyI807fcrxzaA05l_TGO0bvUTiPbyCG3qewP88kzIvQNzmLbpbmSM2HuCGL6IPstaGslSnRycez0ZOKmKzb-qqcWyUQgJCxb1aXMp6OEwbc3p2tU7OZnUkI1fKsD8ZjbL2dsFjkp2bNhh3cW3gZJnpJmzWEt83czXRUoxitdRa5vzs_wle5WbMMbx"
          >
        </div>
      </div>
    </div>
  </header>
</template>
