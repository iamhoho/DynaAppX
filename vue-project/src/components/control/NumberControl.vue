<script setup>
import { ref, watch, onMounted } from 'vue'

const props = defineProps({
    required: {
        required: true
    },
    modelValue: {
    },
    lableName: {
    },
    attributeName: {
    }
}
)

const value = ref(null)

const emit = defineEmits(['update:modelValue']);

watch(() => value.value, (newValue, oldValue) => {
    emit('update:modelValue', Number(newValue));
})

onMounted(() => {
    if (props.modelValue == null || props.modelValue == undefined) {
        value.value = null
    }
    else if (props.modelValue) {
        value.value = props.modelValue;
    }
})
</script>
<template>
    <div style="display: flex; flex-wrap: nowrap; flex-direction: row; margin: 1em; align-items: center;">
        <div class="controlLable">
            <p>{{ lableName }}</p>
            <p v-if="attributeName">{{ attributeName }}</p>
        </div>
        <el-input class="controlItem" v-model="value" placeholder="Please input" type="number" />
        <el-icon style="margin-left: 5px;" size="2em">
            <WarningFilled color="#F56C6C" v-if="required && (modelValue == null || modelValue == '')" />
            <SuccessFilled color="#67C23A" v-else />
        </el-icon>
    </div>
</template>