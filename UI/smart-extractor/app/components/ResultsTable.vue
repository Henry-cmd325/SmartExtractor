<script setup lang="ts">
const props = withDefaults(defineProps<{
  hasPdf?: boolean
  isParsing?: boolean
  feedbackMessage?: string
}>(), {
  hasPdf: false,
  isParsing: false,
  feedbackMessage: ''
})

const emit = defineEmits<{
  exportClicked: []
}>()

const exportLabel = computed(() => {
  if (props.isParsing) {
    return 'Parsing...'
  }

  return 'Export to Excel'
})

const onExportClick = () => {
  if (!props.hasPdf || props.isParsing) {
    return
  }

  emit('exportClicked')
}
</script>

<template>
  <div class="space-y-4">
    <div class="flex justify-between items-end px-1">
      <div>
        <h3 class="font-headline font-bold text-lg text-on-surface">
          Real-time Extraction
        </h3>
        <p class="font-label text-[10px] uppercase tracking-widest text-on-surface-variant">
          Live Parser Stream v2.0.4
        </p>
      </div>
      <button
        type="button"
        class="flex items-center gap-2 border border-outline-variant/20 px-4 py-2 rounded-xl transition-all"
        :class="[
          hasPdf && !isParsing
            ? 'bg-surface-container-high hover:bg-surface-variant text-on-surface'
            : 'bg-surface-container-low text-on-surface-variant cursor-not-allowed opacity-70'
        ]"
        :disabled="!hasPdf || isParsing"
        @click="onExportClick"
      >
        <span class="material-symbols-outlined text-sm">download</span>
        <span class="font-label text-xs uppercase tracking-widest">{{ exportLabel }}</span>
      </button>
    </div>
    <p
      class="font-body text-sm px-1"
      :class="isParsing ? 'text-primary' : 'text-on-surface-variant'"
    >
      {{ feedbackMessage || 'Sube un PDF para habilitar la exportación.' }}
    </p>
    <div class="bg-surface-container-low rounded-xl overflow-hidden border border-outline-variant/10">
      <table class="w-full text-left border-collapse">
        <thead class="bg-surface-container-high/50">
          <tr>
            <th class="px-6 py-4 font-label text-[10px] uppercase tracking-widest text-on-surface-variant">
              Entity ID
            </th>
            <th class="px-6 py-4 font-label text-[10px] uppercase tracking-widest text-on-surface-variant">
              Description
            </th>
            <th class="px-6 py-4 font-label text-[10px] uppercase tracking-widest text-on-surface-variant">
              Value
            </th>
            <th class="px-6 py-4 font-label text-[10px] uppercase tracking-widest text-on-surface-variant">
              Confidence
            </th>
          </tr>
        </thead>
        <tbody class="divide-y divide-outline-variant/5">
          <tr class="hover:bg-surface-container-highest/40 transition-colors">
            <td class="px-6 py-4 font-label text-primary">
              #TRX-902
            </td>
            <td class="px-6 py-4 font-body text-sm text-on-surface">
              Quarterly Revenue Growth
            </td>
            <td class="px-6 py-4 font-body text-sm text-secondary font-bold">
              $1.24M
            </td>
            <td class="px-6 py-4">
              <div class="flex items-center gap-2">
                <div class="flex-1 h-1 bg-surface-container-highest rounded-full overflow-hidden">
                  <div class="h-full bg-secondary w-[98%] shadow-[0_0_8px_rgba(83,221,252,0.4)]" />
                </div>
                <span class="font-label text-[10px] text-secondary">98.2%</span>
              </div>
            </td>
          </tr>
          <tr class="bg-surface-container-high/20 hover:bg-surface-container-highest/40 transition-colors">
            <td class="px-6 py-4 font-label text-primary">
              #TRX-903
            </td>
            <td class="px-6 py-4 font-body text-sm text-on-surface">
              Operating Expenses
            </td>
            <td class="px-6 py-4 font-body text-sm text-secondary font-bold">
              $450.2K
            </td>
            <td class="px-6 py-4">
              <div class="flex items-center gap-2">
                <div class="flex-1 h-1 bg-surface-container-highest rounded-full overflow-hidden">
                  <div class="h-full bg-primary w-[92%] shadow-[0_0_8px_rgba(204,151,255,0.4)]" />
                </div>
                <span class="font-label text-[10px] text-primary">92.4%</span>
              </div>
            </td>
          </tr>
          <tr class="hover:bg-surface-container-highest/40 transition-colors">
            <td class="px-6 py-4 font-label text-primary">
              #TRX-904
            </td>
            <td class="px-6 py-4 font-body text-sm text-on-surface">
              Net Profit Margin
            </td>
            <td class="px-6 py-4 font-body text-sm text-secondary font-bold">
              14.2%
            </td>
            <td class="px-6 py-4">
              <div class="flex items-center gap-2">
                <div class="flex-1 h-1 bg-surface-container-highest rounded-full overflow-hidden">
                  <div class="h-full bg-secondary w-[99%] shadow-[0_0_8px_rgba(83,221,252,0.4)]" />
                </div>
                <span class="font-label text-[10px] text-secondary">99.8%</span>
              </div>
            </td>
          </tr>
        </tbody>
      </table>
    </div>
  </div>
</template>
