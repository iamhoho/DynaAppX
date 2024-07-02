<script setup>
import LookUp from '../components/LookUp.vue';
import SelectFlow from '../components/SelectFlow.vue';
import ActionString from '../components/ActionString.vue';
import ActionBool from '../components/ActionBool.vue';
import ActionNumber from '../components/ActionNumber.vue';
import ActionDate from '../components/ActionDate.vue';
import ActionEntityCollection from '../components/ActionEntityCollection.vue';
import ActionEntity from '../components/ActionEntity.vue';
import ActionEntityReference from '../components/ActionEntityReference.vue';
</script>

<template>
    <div style="display: flex;flex-direction: row; justify-content: center;">
        <select-flow :required="true" :disabled="false" v-model="selectedFlow"></select-flow>
        <look-up v-if="selectedFlow?.primaryentity && selectedFlow.primaryentity != 'none'"
            :logical-Name="selectedFlow?.primaryentity" :required="true" :disabled="selectedFlow == null"
            v-model="selectedRecord"></look-up>
    </div>
    <div style="display: flex;flex-direction: row; justify-content: center;"><el-button type="success"
            @click="invoke">Invoke</el-button></div>
    <div style="display: flex;flex-direction: row; justify-content: center;">{{ response }}</div>
    <div>
        <action-string v-model="actionString" :required="true" att-name="string" :disabled="false"></action-string>
        <action-bool v-model="actionBool" :required="true" att-name="bool" :disabled="false"></action-bool>
        <action-number v-model="actionNumber" :required="true" att-name="number" :disabled="false"></action-number>
        <action-date v-model="actionDatetime" :required="true" att-name="date" :disabled="false"></action-date>
        <action-entity-collection v-model="actionEntityCollection" :required="true" att-name="entityCollection"
            :disabled="false"></action-entity-collection>
        <action-entity v-model="actionEntity" :required="true" att-name="entity" :disabled="false"></action-entity>
        <action-entity-reference v-model="actionEntityReference" :required="true" att-name="entityReference"
            :disabled="false"></action-entity-reference>
    </div>
</template>
