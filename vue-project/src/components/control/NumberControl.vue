<script setup>
import { ref, watch, onMounted } from 'vue'

const props = defineProps({
    required: {
        required: true
    },
    modelValue: {
    },
    attName: {
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
    <div style="display: flex; flex-wrap: nowrap; flex-direction: row; margin: 1em;">
        <p>{{ attName }}</p>
        <el-input v-model="value" placeholder="Please input" type="number" />
        <el-icon size="2em" :color="required && (modelValue == null || modelValue == '') ? '#F56C6C' : '#67C23A'">
            <SuccessFilled />
        </el-icon>
    </div>
</template>